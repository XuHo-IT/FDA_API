# Stop execution if any command fails
set -e

DB_HOST="postgres"
DB_PORT="5432"
API_DLL="FDAAPI.Presentation.FastEndpointBasedApi.dll"

DB_PROJECT_PATH="/src/src/Core/Domain/FDAAPI.Domain.RelationalDb" 


echo "Waiting for PostgreSQL at $DB_HOST:$DB_PORT ..."
while ! nc -z $DB_HOST $DB_PORT; do
  sleep 1
done
echo "PostgreSQL is up and running."


echo "Applying database migrations..."
dotnet tool install --global dotnet-ef --version 9.0.* || true 
export PATH="$PATH:/root/.dotnet/tools"

dotnet ef database update --project $DB_PROJECT_PATH --startup-project /src/src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/FDAAPI.Presentation.FastEndpointBasedApi.csproj --connection "$ConnectionStrings__MyDB"
echo "Database Migrations Applied."


echo "Starting application: $API_DLL"
# The API DLL is located at /app in the final runtime image
dotnet /app/$API_DLL

exec "$@"