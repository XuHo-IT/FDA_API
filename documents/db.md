///////////////////////////////////////////////////////
// USERS & AUTH
///////////////////////////////////////////////////////

Table users {
id uuid [pk]
email varchar(255) [unique, not null]
password_hash varchar(255)
full_name varchar(255)
phone_number varchar(50) [unique]
avartar_url text
provider varchar(50) [default: 'local'] // local, google, clerk
status varchar(20) [default: 'ACTIVE'] // ACTIVE, BANNED
last_login_at timestamptz
phone_verified_at timestamptz
email_verified_at timestamptz

created_by uuid [not null]
created_at timestamptz [not null]
updated_by uuid [not null]
updated_at timestamptz [not null]

indexes {
(email) [unique, name:'ix_users_email']
(phone_number) [unique, name:'ix_users_phone']
(status) [name:'ix_users_status']
}

Note: '''
- phone_verified_at: Timestamp when phone is verified via OTP
- email_verified_at: Timestamp when email is verified
- provider: Authentication provider (local, google, clerk)
- Auto-registration: Citizens auto-created on first phone+OTP login
'''
}

Table roles {
id uuid [pk]
code varchar(50) [unique, not null] // ADMIN, MODERATOR, USER
name varchar(100) [not null]

indexes {
(code) [unique, name:'ix_roles_code']
}

Note: '''
Seed Data:
- ADMIN: Administrator
- MODERATOR: Moderator Government Officer (previously GOV)
- USER: Citizen User
'''
}

Table user_roles {
id uuid [pk]
user_id uuid [ref: > users.id, delete: cascade]
role_id uuid [ref: > roles.id, delete: cascade]

indexes {
(user_id, role_id) [unique, name:'uq_user_roles']
(user_id) [name:'ix_user_roles_user']
(role_id) [name:'ix_user_roles_role']
}

Note: '''
Many-to-many relationship between users and roles.
Cascade delete: Deleting user removes all role assignments.
'''
}

Table refresh_tokens {
id uuid [pk]
user_id uuid [ref: > users.id, delete: cascade]
token varchar(255) [unique, not null]
expires_at timestamptz [not null]
is_revoked boolean [default: false]
revoked_at timestamptz
device_info text
ip_address varchar(50)

created_by uuid [not null]
created_at timestamptz [not null]

indexes {
(token) [unique, name:'ix_refresh_tokens_token']
(user_id) [name:'ix_refresh_tokens_user']
(is_revoked) [name:'ix_refresh_tokens_revoked']
}

Note: '''
- Token rotation: Old token revoked when issuing new token
- Expires in 7 days
- Tracks device info and IP for security audit
'''
}

Table otp_codes {
id uuid [pk]
phone_number varchar(50) [not null]
code varchar(6) [not null]
expires_at timestamptz [not null]
is_used boolean [default: false]
used_at timestamptz
attempt_count int [default: 0]

created_by uuid [not null]
created_at timestamptz [not null]

indexes {
(phone_number) [name:'ix_otp_codes_phone']
(phone_number, is_used) [name:'ix_otp_codes_phone_used']
}

Note: '''
- Mock OTP in development: "123456"
- Expires in 5 minutes
- Maximum 3 attempts tracked
- Marked as used after successful login
'''
}

///////////////////////////////////////////////////////
// LOCATION / MAP / AREAS
///////////////////////////////////////////////////////

Table areas {
id uuid [pk]
user_id uuid [not null, ref: > users.id]
name varchar(255)
latitude numeric(10,6)
longitude numeric(10,6)
radius_m int // bán kính để theo dõi
address_text text

created_at timestamptz [not null]
updated_at timestamptz

indexes {
(user_id) [name:'ix_areas_user']
(latitude, longitude) [name:'ix_areas_geo']
}
}

// Table flood_routes_cache {
// id uuid [pk]
// user_id uuid [ref: > users.id]
// from_lat numeric(10,6)
// from_lng numeric(10,6)
// to_lat numeric(10,6)
// to_lng numeric(10,6)
// safe_route_json jsonb
// cached_at timestamptz [not null]

// indexes {
// (user_id, cached_at) [name:'ix_route_user_time']
// }
// }

///////////////////////////////////////////////////////
// IOT SENSORS & STATIONS
///////////////////////////////////////////////////////

Table stations {
id uuid [pk]
code varchar(50) [unique, not null] // ST_DN_01
name varchar(255)
location_desc text
latitude numeric(10,6)
longitude numeric(10,6)
road_name varchar(255)
direction varchar(100) // upstream/downstream/road section
status varchar(20) // active, offline, maintenance
installed_at timestamptz
last_seen_at timestamptz

created_at timestamptz [not null]
updated_at timestamptz

indexes {
(status) [name:'ix_station_status']
(latitude, longitude) [name:'ix_station_geo']
}
}

Table devices {
id uuid [pk]
station_id uuid [ref: > stations.id]
hardware_id varchar(100) [unique] // ESP32 chip id
firmware_version varchar(50)
battery_level numeric(5,2)
ip_address varchar(50)
status varchar(20) // online/offline/fault
last_seen_at timestamptz

created_at timestamptz [not null]

indexes {
(station_id) [name:'ix_device_station']
(status) [name:'ix_device_status']
}
}

Table devices_logs {
id uuid [pk]
device_id uuid [ref: > devices.id]
station_id uuid [ref: > stations.id]
event_type varchar(50) // heartbeat, error, restart
message text
created_at timestamptz [not null]

indexes {
(device_id, created_at)
(station_id, created_at)
(event_type)
}
}

///////////////////////////////////////////////////////
// SENSORS & WATER-LEVEL READINGS
///////////////////////////////////////////////////////

Table sensors {
id uuid [pk]
station_id uuid [ref: > stations.id]
type varchar(50) // water_level, rain
code varchar(50) [unique]
calibration numeric(10,4) // mm offset
status varchar(20)

created_at timestamptz
updated_at timestamptz
}

Table sensor_readings {
id uuid [pk]
sensor_id uuid [ref: > sensors.id]
station_id uuid [ref: > stations.id]
measured_at timestamptz [not null]
raw_value numeric(14,4)
calibrated_val numeric(14,4)
quality_flag varchar(20) // ok/suspect/bad
raw_payload jsonb

created_at timestamptz [not null]

indexes {
(sensor_id, measured_at) [name:'ix_readings_sensor_time']
(station_id, measured_at) [name:'ix_readings_station_time']
(measured_at) [name:'ix_readings_time']
}
}

Table sensor_daily_agg {
id uuid [pk]
station_id uuid [ref: > stations.id]
date date
max_level numeric(14,4)
min_level numeric(14,4)
avg_level numeric(14,4)
rainfall_total numeric(14,4)

created_at timestamptz [not null]

indexes {
(station_id, date) [unique]
}
}

///////////////////////////////////////////////////////
// ALERT SYSTEM
///////////////////////////////////////////////////////

Table alert_rules {
id uuid [pk]
station_id uuid [ref: > stations.id]
name varchar(255)
rule_type varchar(50) // threshold, rate_change
threshold_value numeric(14,4)
// operator varchar(10) // >=, >
duration_min int // vượt X phút
severity varchar(20) // info/warning/critical
is_active boolean [default: true]

created_at timestamptz
updated_at timestamptz
}

Table alerts {
id uuid [pk]
alert_rule_id uuid [ref: > alert_rules.id]
station_id uuid [ref: > stations.id]
triggered_at timestamptz [not null]
resolved_at timestamptz
status varchar(20) // open, resolved
severity varchar(20)
current_value numeric(14,4)
message text

created_at timestamptz
updated_at timestamptz

indexes {
(station_id, triggered_at)
(status)
(severity)
}
}

Table user_alert_subscriptions {
id uuid [pk]
user_id uuid [ref: > users.id]
area_id uuid [ref: > areas.id]
station_id uuid [ref: > stations.id]
min_severity varchar(20) // warning/critical

created_at timestamptz

indexes {
(user_id, station_id) [unique]
}
}

Table notification_logs {
id uuid [pk]
user_id uuid [ref: > users.id]
alert_id uuid [ref: > alerts.id]
channel varchar(50) // push, sms, email
destination varchar(255)
content text
sent_at timestamptz
status varchar(20) // success, failed

indexes {
(user_id, sent_at)
(alert_id)
}
}

///////////////////////////////////////////////////////
// ADMIN & ANALYTICS
///////////////////////////////////////////////////////

Table admin_actions {
id uuid [pk]
admin_id uuid [ref: > users.id]
action_type varchar(50) // update_sensor, create_station,...
entity varchar(50)
entity_id uuid
detail_json jsonb
created_at timestamptz [not null]

indexes {
(admin_id, created_at)
}
}

Table flood_statistics {
id uuid [pk]
station_id uuid [ref: > stations.id]
date date
avg_depth_m numeric(14,4)
max_depth_m numeric(14,4)
flood_hours int
created_at timestamptz

indexes {
(station_id, date)
}
}

// Table export_logs {
// id uuid [pk]
// user_id uuid [ref: > users.id]
// type varchar(50) // csv, pdf
// filters_json jsonb
// created_at timestamptz

// indexes {
// (user_id, created_at)
// }
// }

///////////////////////////////////////////////////////
// PRICING SYSTEM – Subscription Upgrade
///////////////////////////////////////////////////////

Table pricing_plans {
id uuid [pk]
code varchar(50) [unique] // FREE, PRO, GOV
name varchar(100)
description text
price_month numeric(10,2)
price_year numeric(10,2)
is_active boolean [default: true]
sort_order int

created_at timestamptz
updated_at timestamptz

indexes {
(is_active)
(sort_order)
}
}

Table pricing_features {
id uuid [pk]
plan_id uuid [ref: > pricing_plans.id]
feature_key varchar(100)
feature_name varchar(255)
feature_value varchar(255)
description text

created_at timestamptz

indexes {
(plan_id)
(feature_key)
}
}

Table user_subscriptions {
id uuid [pk]
user_id uuid [ref: > users.id]
plan_id uuid [ref: > pricing_plans.id]
start_date timestamptz
end_date timestamptz
renew_mode varchar(20)
status varchar(20)
cancel_reason text

created_at timestamptz
updated_at timestamptz

indexes {
(user_id, status)
(plan_id)
}
}

Table subscription_payments {
id uuid [pk]
subscription_id uuid [ref: > user_subscriptions.id]
user_id uuid [ref: > users.id]
plan_id uuid [ref: > pricing_plans.id]
amount numeric(10,2)
currency varchar(10)
payment_method varchar(50)
transaction_id varchar(255)
status varchar(20)
paid_at timestamptz
created_at timestamptz

indexes {
(user_id)
(subscription_id)
(status)
}
}

Table subscription_usage {
id uuid [pk]
user_id uuid [ref: > users.id]
usage_key varchar(100)
used_value int
reset_period varchar(20)
period_start timestamptz
period_end timestamptz
created_at timestamptz

indexes {
(user_id, usage_key)
}
}

Table prediction_runs {
id uuid [pk]
station_id uuid [ref: > stations.id]
provider varchar(50)  
 model_name varchar(100)  
 horizon_max int  
 triggered_by varchar(50)  
 requested_at timestamptz  
 responded_at timestamptz  
 status varchar(20)  
 error_message text  
 request_payload jsonb  
 response_raw jsonb

indexes {
(station_id, requested_at)
(status)
}
}

Table prediction_results {
id uuid [pk]
prediction_run_id uuid [ref: > prediction_runs.id]
station_id uuid [ref: > stations.id]
predicted_at timestamptz  
 horizon_minutes int  
 water_level_pred numeric(14,4)
risk_level varchar(20)
created_at timestamptz

indexes {
(station_id, predicted_at)
(prediction_run_id)
(station_id, risk_level, predicted_at)
}
}
