# Socket Feature Template

## Flow: Emit → Listen

```
UI Component
    │  const { mutate } = useToggleEventAgendaAutoStatus(eventId)
    │  mutate({ isEnable: true })
    │
    ▼
hooks/mutations/agenda.mutation.ts
    │  // Khởi tạo socket hook tại component level
    │  const { mutate: toggleEventAgendaAutoStatusSocket }
    │    = useToggleEventAgendaAutoStatusSocket(eventId)   ← gọi hook ở đây
    │
    │  useMutation({
    │    mutationFn({ isEnable }) {
    │      toggleEventAgendaAutoStatusSocket(isEnable)     ← gọi emit qua socket hook
    │      return Promise.resolve({ isAgendaAuto: isEnable })
    │    },
    │    onSuccess(d) {
    │      queryClient.setQueryData(QUERY_KEYS.agenda.autoStatus(eventId), d)
    │    }
    │  })
    │
    ▼
hooks/sockets/agenda.socket.ts
    │  useToggleEventAgendaAutoStatusSocket(eventId)
    │  └─ mutate(isEnable) {
    │       toggleEventAgendaAutoStatus(eventId, isEnable)   ← gọi gateway
    │       queryClient.invalidateQueries(...)
    │     }
    │
    ▼
services/gateways/agenda-gateway.service.ts
    │  toggleEventAgendaAutoStatus(eventId, isEnable)
    │  └─ socketClient.emit('agenda.toggleEventAgendaAuto', { eventId, isEnable })
    │
    ▼
lib/socket-client.ts  →  [Server]  →  broadcast event
    │
    ▼
services/gateways/agenda-gateway.service.ts
    │  onEventAgendaAutoStatusToggled(callback)
    │  └─ socketClient.on('agenda.eventAgendaAutoToggled', callback)
    │     return () => socketClient.off(...)
    │
    ▼
hooks/sockets/agenda.socket.ts
    │  useGetEventAgendaAutoStatusToggledSocket({ callback })
    │  └─ useSocketSubscription({ subscription: onEventAgendaAutoStatusToggled, callback })
    │
    ▼
hooks/sockets/common/use-socket-subscription.ts   ← SHARED, không tạo mới
    │  useSocketSubscription<T>({
    │    subscription,    // SubscriptionFunction<T> = (callback) => UnsubscribeFunction
    │    callback,        // SocketCallback<T>        = (data: T) => void
    │    enabled?,        // boolean — default: true
    │    dependencies?,   // any[]  — re-subscribe khi thay đổi
    │  })
    │  useEffect(() => {
    │    if (!enabled) return
    │    unsubscribeRef.current = subscription(memoizedCallback)  ← subscribe khi mount
    │    return () => unsubscribeRef.current?.()                  ← unsubscribe khi unmount
    │  }, [subscription, memoizedCallback, enabled, ...dependencies])
    │
    ▼
UI Component  ← callback được gọi, cập nhật cache/state
```

---

## Cấu trúc file

```
src/
├── models/
│   └── session/
│       ├── entity/
│       │   ├── enum/
│       │   │   └── session-status.enum.ts        [1] TẠO MỚI
│       │   └── session.entity.ts                 [2] TẠO MỚI
│       └── schema/
│           ├── create-session.dto.ts             [3] TẠO MỚI
│           └── update-session.dto.ts             [4] TẠO MỚI
│
├── services/
│   └── gateways/
│       └── session-gateway.service.ts            [5] TẠO MỚI
│
├── hooks/
│   ├── sockets/
│   │   ├── common/
│   │   │   └── use-socket-subscription.ts        ← DÙNG CHUNG, KHÔNG TẠO
│   │   └── session.socket.ts                     [6] TẠO MỚI
│   └── mutations/
│       └── session.mutation.ts                   [7] TẠO MỚI
│
└── constants/
    └── query-keys.ts                             ← THÊM sessions key vào file có sẵn
```

---

## [1] `models/session/entity/enum/session-status.enum.ts`

```ts
export enum ESessionStatus {
  NOT_STARTED = "NOT_STARTED",
  ON_GOING = "ON_GOING",
  FINISHED = "FINISHED",
}
```

---

## [2] `models/session/entity/session.entity.ts`

```ts
import { ESessionStatus } from "./enum/session-status.enum";

export interface SessionEntity {
  id: number;
  eventId: number;
  title: string;
  description: string;
  startDate: Date;
  endDate: Date;
  status: ESessionStatus;
  autoStatusEnabled: boolean;
}
```

---

## [3] `models/session/schema/create-session.dto.ts`

```ts
export interface CreateSessionDto {
  eventId: number;
  title: string;
  description: string;
  startDate: Date;
  endDate: Date;
  autoStatusEnabled: boolean;
}
```

---

## [4] `models/session/schema/update-session.dto.ts`

```ts
export interface UpdateSessionDto {
  title?: string;
  description?: string;
  startDate?: Date;
  endDate?: Date;
  autoStatusEnabled?: boolean;
}
```

---

## [5] `services/gateways/session-gateway.service.ts`

```ts
import socketClient from "@/lib/socket-client";
import { SocketCallback } from "@/types/socket";
import { SessionEntity } from "@/models/session/entity/session.entity";
import { CreateSessionDto } from "@/models/session/schema/create-session.dto";
import { UpdateSessionDto } from "@/models/session/schema/update-session.dto";

// Socket event names (server → client)
export const SESSION_EVENTS = {
  SESSION_CREATED: "session.created",
  SESSION_UPDATED: "session.updated",
  SESSION_DELETED: "session.deleted",
  SESSION_AUTO_STATUS_TOGGLED: "session.autoStatusToggled",
};

// Socket message names (client → server)
export const SESSION_MESSAGES = {
  CREATE_SESSION: "session.create",
  UPDATE_SESSION: "session.update",
  DELETE_SESSION: "session.delete",
  TOGGLE_SESSION_AUTO_STATUS: "session.toggleAutoStatus",
};

// ─── Emit functions ───────────────────────────────────────────────────────────

export const createSession = (data: CreateSessionDto) => {
  try {
    socketClient.emit(SESSION_MESSAGES.CREATE_SESSION, data);
  } catch (error) {
    console.error(`Error emitting ${SESSION_MESSAGES.CREATE_SESSION}:`, error);
    throw new Error("Failed to create session");
  }
};

export const updateSession = (sessionId: number, data: UpdateSessionDto) => {
  try {
    socketClient.emit(SESSION_MESSAGES.UPDATE_SESSION, {
      sessionId,
      updateSessionDto: data,
    });
  } catch (error) {
    console.error(`Error emitting ${SESSION_MESSAGES.UPDATE_SESSION}:`, error);
    throw new Error("Failed to update session");
  }
};

export const deleteSession = (sessionId: number, eventId: number) => {
  try {
    socketClient.emit(SESSION_MESSAGES.DELETE_SESSION, { sessionId, eventId });
  } catch (error) {
    console.error(`Error emitting ${SESSION_MESSAGES.DELETE_SESSION}:`, error);
    throw new Error("Failed to delete session");
  }
};

export const toggleSessionAutoStatus = (
  sessionId: number,
  isEnable: boolean,
) => {
  try {
    socketClient.emit(SESSION_MESSAGES.TOGGLE_SESSION_AUTO_STATUS, {
      sessionId,
      isEnable,
    });
  } catch (error) {
    console.error(
      `Error emitting ${SESSION_MESSAGES.TOGGLE_SESSION_AUTO_STATUS}:`,
      error,
    );
    throw new Error("Failed to toggle session auto status");
  }
};

// ─── Listen functions ─────────────────────────────────────────────────────────

export const onSessionCreated = (callback: SocketCallback<SessionEntity>) => {
  try {
    socketClient.on(SESSION_EVENTS.SESSION_CREATED, callback);
    return () => socketClient.off(SESSION_EVENTS.SESSION_CREATED, callback);
  } catch (error) {
    console.error(
      `Error adding ${SESSION_EVENTS.SESSION_CREATED} listener:`,
      error,
    );
    throw new Error("Failed to listen for created session");
  }
};

export const onSessionUpdated = (callback: SocketCallback<SessionEntity>) => {
  try {
    socketClient.on(SESSION_EVENTS.SESSION_UPDATED, callback);
    return () => socketClient.off(SESSION_EVENTS.SESSION_UPDATED, callback);
  } catch (error) {
    console.error(
      `Error adding ${SESSION_EVENTS.SESSION_UPDATED} listener:`,
      error,
    );
    throw new Error("Failed to listen for updated session");
  }
};

export const onSessionDeleted = (
  callback: SocketCallback<{ id: number; eventId: number }>,
) => {
  try {
    socketClient.on(SESSION_EVENTS.SESSION_DELETED, callback);
    return () => socketClient.off(SESSION_EVENTS.SESSION_DELETED, callback);
  } catch (error) {
    console.error(
      `Error adding ${SESSION_EVENTS.SESSION_DELETED} listener:`,
      error,
    );
    throw new Error("Failed to listen for deleted session");
  }
};

export const onSessionAutoStatusToggled = (
  callback: SocketCallback<SessionEntity>,
) => {
  try {
    socketClient.on(SESSION_EVENTS.SESSION_AUTO_STATUS_TOGGLED, callback);
    return () =>
      socketClient.off(SESSION_EVENTS.SESSION_AUTO_STATUS_TOGGLED, callback);
  } catch (error) {
    console.error(
      `Error adding ${SESSION_EVENTS.SESSION_AUTO_STATUS_TOGGLED} listener:`,
      error,
    );
    throw new Error("Failed to listen for session auto status toggled");
  }
};
```

---

## [6] `hooks/sockets/session.socket.ts`

```ts
import { useQueryClient } from "@tanstack/react-query";

import { QUERY_KEYS } from "@/constants/query-keys";
import { useSocketSubscription } from "@/hooks/sockets/common/use-socket-subscription";
import { SessionEntity } from "@/models/session/entity/session.entity";
import { CreateSessionDto } from "@/models/session/schema/create-session.dto";
import { UpdateSessionDto } from "@/models/session/schema/update-session.dto";
import { ListenEventParams } from "@/types/socket";
import {
  createSession,
  updateSession,
  deleteSession,
  toggleSessionAutoStatus,
  onSessionCreated,
  onSessionUpdated,
  onSessionDeleted,
  onSessionAutoStatusToggled,
} from "@/services/gateways/session-gateway.service";

// ─── Emit hooks ───────────────────────────────────────────────────────────────

export const useCreateSessionSocket = (eventId: number) => {
  const queryClient = useQueryClient();

  const mutate = (data: CreateSessionDto) => {
    createSession(data);
    queryClient.invalidateQueries({
      queryKey: [QUERY_KEYS.sessions.forEvent(eventId)],
    });
  };

  return { mutate };
};

export const useUpdateSessionSocket = (eventId: number) => {
  const queryClient = useQueryClient();

  const mutate = (sessionId: number, data: UpdateSessionDto) => {
    updateSession(sessionId, data);
    queryClient.invalidateQueries({
      queryKey: [QUERY_KEYS.sessions.forEvent(eventId)],
    });
  };

  return { mutate };
};

export const useDeleteSessionSocket = (eventId: number) => {
  const queryClient = useQueryClient();

  const mutate = (sessionId: number) => {
    deleteSession(sessionId, eventId);
    queryClient.setQueryData<SessionEntity[]>(
      QUERY_KEYS.sessions.forEvent(eventId),
      (old = []) => old.filter((s) => s.id !== sessionId),
    );
  };

  return { mutate };
};

export const useToggleSessionAutoStatusSocket = (eventId: number) => {
  const queryClient = useQueryClient();

  const mutate = (sessionId: number, isEnable: boolean) => {
    toggleSessionAutoStatus(sessionId, isEnable);
    queryClient.invalidateQueries({
      queryKey: [QUERY_KEYS.sessions.forEvent(eventId)],
    });
  };

  return { mutate };
};

// ─── Listen hooks ─────────────────────────────────────────────────────────────

export const useGetCreatedSessionSocket = ({
  callback,
  ...params
}: ListenEventParams<SessionEntity>) => {
  useSocketSubscription({
    ...params, // enabled?, dependencies?
    subscription: onSessionCreated, // từ gateway → trả về unsubscribe fn
    callback, // SocketCallback<SessionEntity>
  });
  // Không return gì — tự subscribe khi mount, unsubscribe khi unmount
};

export const useGetUpdatedSessionSocket = ({
  callback,
  ...params
}: ListenEventParams<SessionEntity>) => {
  useSocketSubscription({
    ...params,
    subscription: onSessionUpdated,
    callback,
  });
};

export const useGetDeletedSessionSocket = ({
  callback,
  ...params
}: ListenEventParams<{ id: number; eventId: number }>) => {
  useSocketSubscription({
    ...params,
    subscription: onSessionDeleted,
    callback,
  });
};

export const useGetSessionAutoStatusToggledSocket = ({
  callback,
  ...params
}: ListenEventParams<SessionEntity>) => {
  useSocketSubscription({
    ...params,
    subscription: onSessionAutoStatusToggled,
    callback,
  });
};
```

---

## [7] `hooks/mutations/session.mutation.ts`

```ts
import { useMutation, useQueryClient } from "@tanstack/react-query";

import { QUERY_KEYS } from "@/constants/query-keys";
import { MutationParams } from "@/types/query";
import { SessionEntity } from "@/models/session/entity/session.entity";
import { CreateSessionDto } from "@/models/session/schema/create-session.dto";
import { UpdateSessionDto } from "@/models/session/schema/update-session.dto";
import {
  useCreateSessionSocket,
  useUpdateSessionSocket,
  useDeleteSessionSocket,
  useToggleSessionAutoStatusSocket,
} from "@/hooks/sockets/session.socket";

type UpdateSessionVariables = { sessionId: number; data: UpdateSessionDto };
type DeleteSessionVariables = { sessionId: number };
type ToggleAutoStatusVariables = { sessionId: number; isEnable: boolean };

export const useCreateSession = (
  eventId: number,
  params?: MutationParams<SessionEntity, CreateSessionDto>,
) => {
  const queryClient = useQueryClient();
  const { mutate: createSessionSocket } = useCreateSessionSocket(eventId);

  return useMutation({
    ...params,
    mutationFn: (data) => {
      createSessionSocket(data);
      return Promise.resolve({} as SessionEntity);
    },
    onSuccess: (d, v, r, c) => {
      params?.onSuccess?.(d, v, r, c);
      queryClient.invalidateQueries({
        queryKey: [QUERY_KEYS.sessions.forEvent(eventId)],
      });
    },
  });
};

export const useUpdateSession = (
  eventId: number,
  params?: MutationParams<SessionEntity, UpdateSessionVariables>,
) => {
  const { mutate: updateSessionSocket } = useUpdateSessionSocket(eventId);

  return useMutation({
    ...params,
    mutationFn: ({ sessionId, data }) => {
      updateSessionSocket(sessionId, data);
      return Promise.resolve({} as SessionEntity);
    },
    onSuccess: (d, v, r, c) => {
      params?.onSuccess?.(d, v, r, c);
    },
    onError: (error, variables, r, c) => {
      params?.onError?.(error, variables, r, c);
    },
  });
};

export const useDeleteSession = (
  eventId: number,
  params?: MutationParams<string, DeleteSessionVariables>,
) => {
  const queryClient = useQueryClient();
  const { mutate: deleteSessionSocket } = useDeleteSessionSocket(eventId);

  return useMutation({
    ...params,
    mutationFn: async ({ sessionId }) => {
      deleteSessionSocket(sessionId);
      return Promise.resolve(`Session ${sessionId} deleted`);
    },
    onSuccess: (d, v, r, c) => {
      params?.onSuccess?.(d, v, r, c);
      queryClient.setQueryData<SessionEntity[]>(
        QUERY_KEYS.sessions.forEvent(eventId),
        (old) => old?.filter((s) => s.id !== v.sessionId) ?? [],
      );
    },
    onError: (error, variables, r, c) => {
      params?.onError?.(error, variables, r, c);
      queryClient.invalidateQueries({
        queryKey: [QUERY_KEYS.sessions.forEvent(eventId)],
      });
    },
  });
};

export const useToggleSessionAutoStatus = (
  eventId: number,
  params?: MutationParams<SessionEntity, ToggleAutoStatusVariables>,
) => {
  const queryClient = useQueryClient();
  // Khởi tạo socket hook tại component level (không được gọi trong mutationFn)
  const { mutate: toggleSessionAutoStatusSocket } =
    useToggleSessionAutoStatusSocket(eventId);

  return useMutation({
    ...params,
    mutationFn: ({ sessionId, isEnable }) => {
      toggleSessionAutoStatusSocket(sessionId, isEnable);
      return Promise.resolve({
        id: sessionId,
        autoStatusEnabled: isEnable,
      } as SessionEntity);
    },
    onSuccess: (d, v, r, c) => {
      params?.onSuccess?.(d, v, r, c);
      queryClient.setQueryData<SessionEntity[]>(
        QUERY_KEYS.sessions.forEvent(eventId),
        (old = []) =>
          old.map((s) =>
            s.id === v.sessionId ? { ...s, autoStatusEnabled: v.isEnable } : s,
          ),
      );
    },
  });
};
```

---

## `constants/query-keys.ts` — thêm vào object QUERY_KEYS

```ts
sessions: {
  all: () => ['sessions'],
  forEvent: (eventId: number) => [...QUERY_KEYS.sessions.all(), eventId],
},
```

import { useCallback, useEffect, useRef } from 'react';

import { SocketCallback, SubscriptionFunction, UnsubscribeFunction } from '@/types/socket';
import { Maybe } from '@/types/util';

export interface UseSocketSubscriptionOptions<T> {
subscription: SubscriptionFunction<T>; // fn từ gateway: (callback) => unsubscribe
callback: SocketCallback<T>; // (data: T) => void
enabled?: boolean; // default: true — tắt listen có điều kiện
dependencies?: any[]; // re-subscribe khi thay đổi
}

export function useSocketSubscription<T>({
subscription,
callback,
enabled = true,
dependencies = [],
}: UseSocketSubscriptionOptions<T>) {
const memoizedCallback = useCallback(callback, [callback]);
const unsubscribeRef = useRef<Maybe<UnsubscribeFunction>>(null);

useEffect(() => {
if (!enabled) {
return;
}

    unsubscribeRef.current = subscription(memoizedCallback);  // ← subscribe khi mount

    return () => {
      if (unsubscribeRef.current) {
        unsubscribeRef.current();          // ← unsubscribe khi unmount
        unsubscribeRef.current = null;
      }
    };

}, [subscription, memoizedCallback, enabled, ...dependencies]);
}
