# GeoStreet-Service

GeoStreet-Service is a geospatial web service built with ASP.NET Core, leveraging PostgreSQL with optional PostGIS support for spatial data operations. This project enables storing, querying, and manipulating geographical data such as streets and coordinates.

---

## Features

- **Geometry Management**:
  - Store and manage street geometries as `LineString`.
  - Add points to streets dynamically at the start or end.
  - Flexible geometry storage: supports both PostGIS and plain text (`Well-Known Text` - WKT).

- **Concurrency Handling**:
  - Ensures robust handling of concurrent updates to street geometries.

- **Database Configurations**:
  - Supports PostgreSQL with PostGIS for advanced spatial operations.
  - Offers a fallback option to store geometries as plain text.

---

## Testing
The project includes comprehensive tests to specifically ensure the concurrency and operation-level handling.

### Highlights

- **Concurrency Testing**:
  - Simulates concurrent updates to the geometry of a street.
  - Ensures data integrity by validating transaction behavior and detecting potential race conditions.
  - Tests both database-level (`PostGIS`) and code-level operations for adding points to streets.

- **PostGIS and Text Geometry**:
  - The project dynamically switches between PostGIS-based geometry handling and plain text storage (using `Well-Known Text` - WKT).
  - Concurrency tests validate both storage mechanisms for consistency and correctness.

- **Integration Testing**:
  - Uses Docker containers to spin up a PostgreSQL/PostGIS database for realistic integration tests.
  - Includes database schema validation, ensuring migrations and model configurations are correctly applied.

---

## Configuration Options

### Geometry Storage
The project supports dynamic switching between PostGIS-based geometry storage and plain text (`Well-Known Text` - WKT) storage for geometries.

This behavior is controlled by the `UsePostGIS` flag in `appsettings.json`:

```json
"SpatialSettings": {
  "UsePostGIS": true, // Set to true for PostGIS, false for plain text (WKT) geometry storage
  "DefaultSRID": 4326
}
```

#### When UsePostGIS is true:

- The application utilizes PostgreSQL with PostGIS for advanced geospatial operations.
- Suitable for production environments requiring high-performance spatial queries.

#### When UsePostGIS is false:

- Geometry is stored as plain text (WKT) in the database.
- Useful for environments without PostGIS or when geospatial operations are minimal.

# Setup Instructions

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/)
- [Docker](https://www.docker.com/) (for running PostgreSQL/PostGIS container)
- [PostgreSQL](https://www.postgresql.org/)
- PostGIS extension installed on your PostgreSQL instance

Clone the Repository
```bash
git clone https://github.com/HamzaFarooqArif/GeoStreet-Service.git
cd GeoStreet-Service
```

## (Method 1) Docker compose
```bash
docker-compose up
```
Following will be the application's URL which you can open in your browser.
```bash
http://localhost:8088/index.html
```
Make sure you apply database migrations with the following endpoint in the application.
```bash
/ApplyDatabaseMigrations
```

## (Method 2) Docker build
**1.** Make sure PostgreSQL and Docker are running.<br />

**2.** Replace the connection string of your server in appsettings.Development.json e.g.,<br />
```bash
"ConnectionStrings": {
  "WebApiDatabase": "<your-postgreSQL-connection-string>"
},
```

**3.** Build the image.<br />
```bash
docker build --rm -t geostreet.api:latest .
```

**3.** Run the image in container.<br />
```bash
docker run --rm -p 8088:8080 -e ASPNETCORE_URLS="http://+:8080" geostreet.api:latest
```
Following will be the application's URL which you can open in your browser.
```bash
http://localhost:8088/index.html
```
Make sure you apply database migrations with the following endpoint in the application.
```bash
/ApplyDatabaseMigrations
```

## (Method 3) Local Development Mode
**1.** Make sure PostgreSQL is running<br />

**2.** Replace the connection string of your server in appsettings e.g.,<br />
```bash
"ConnectionStrings": {
  "WebApiDatabase": "<your-postgreSQL-connection-string>"
},
```

**3.** (Optional) Apply database migrations.<br />
```bash
cd GeoStreet.API
rm -r Data/Migrations // Delete previous migrations if any
dotnet ef database drop // Drop the previous database
dotnet ef migrations add InitialCreate -o Data/Migrations // Create new migration
dotnet ef database update // Apply migration
```
The application also has the following endpoint to apply migrations under _Setup controller.
```bash
/ApplyDatabaseMigrations
```

**4.** Run the application<br />
```bash
dotnet run
```
Following will be the application's URL which you can open in your browser.
```bash
http://localhost:5108/swagger/index.html
```

## (Method 4) Deployment on kubernetes using minikube
**1.** Install minikube<br />
- [minikube](https://minikube.sigs.k8s.io/docs/start/)
```bash
start minikube
``` 

**2.** Navigate to kubernetes folder.<br />
```bash
cd kubernetes
```

**3.** Clean any previous deployments, services and persistent volumes.<br />
```bash
kubectl delete pods --all
kubectl delete deployment geostreet-deployment
kubectl delete deployment postgres-deployment
kubectl delete service geostreet-service
kubectl delete service db-service
kubectl delete pvc postgres-pvc
```

**4.** Build the image under your docker username.<br />
```bash
docker build -t <your-docker-username>/geostreet-api:latest .
```

**5.** Login in docker.<br />
```bash
docker login
```

**6.** Push the image to docker hub.<br />
```bash
docker push <your-docker-username>/geostreet-api:latest
```

**7.** Replace the docker username in geostreet-deployment.yaml <br />
```bash
image: <your-docker-username>/geostreet-api:latest
```

**8.** Apply deployments.<br />
```bash
kubectl apply -f postgres-pvc.yaml
kubectl apply -f postgres-deployment.yaml
kubectl apply -f postgres-service.yaml

kubectl apply -f geostreet-deployment.yaml
kubectl apply -f geostreet-service.yaml
```

**9.** Verify the deployments, services and pods.<br />
```bash
kubectl get deployments
kubectl get services
kubectl get pods
```

**10.** Start the application<br />
```bash
minikube service geostreet-service
```
---
