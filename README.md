### Main Tasks
1. Build two independant microservices.
2. Implement RESTful APIs for user and order management.
4. Use event-driven communication between services (cross-service communication).
5. Containerize both services.

### Additional Tasks
1. Add service health checks
2. Add logging.
3. Use TDD approach for a Comprehensive test coverage.

### Assumptions
1. Should the services have authentication and authorization? => Assumed no for simplicity.
2. What to do with the events => 
    1. Assumed `UserCreated` event is notifying Order service to keep track of userIds for validation. Otherwise order should not be created.
    2. Assumed `OrderCreated` event is notifying User service to keep the order count for a particular user. Can be extended to send notifications via another service.

### Highlevel Technical Design
1. Use ASP.NET Core Web API projects (.NET 8).
2. USe single GitHub repo, single solution with four projects(Not real world style - just to make it easier to clone, run and review the solution).
3. Use EF Core with in-memory database for data storage.
4. Use Confluent.Kafka for events handling.
6. Add a shared class library to keep common functionalities. 
7. Add a unit tests project to cover all three projects(Not real world style).
8. Add Logging to log important events by each services.
8. Use Docker for containarisation with docker-compose.
9. Add Swagger support for APIs.

### Highlevel Functional Design
1. User Service
    - Expose REST endpoints to create and get users.
    - Validate user input.
    - Store user data in an in-memory database.
    - Publish `UserCreated` event on user creation.
    - Subscribe to `OrderCreated` events to update the order count for users.
    - Increment orders count per user.
2. Order Service
    - Expose endpoints to create and get orders.
    - Validate order input.
    - Store order data in an in-memory database.
    - Publish `OrderCreated` event on order creation.
    - Subscribe to `UserCreated` events to validate user existence before creating an order.
3. Shared Library
    - Define common models for User and Order.
    - Define event models for `UserCreated` and `OrderCreated`.
    - Implement event producer and consumer utilities.

## Trade-offs

1. Used in-memory database instead of a persistent database for simplicity and ease of setup.
2. Kept both services in a single repository for easier review, though in real-world scenarios, they would be in separate repositories.
3. Limited error handling and validation to essential checks to keep the implementation straightforward.
4. Used basic logging instead of a full-fledged logging framework for simplicity.


## Setup Instructions

### Prerequisites
- **.NET 8 SDK** 
- **Docker Desktop**
- **Git**

### Steps to Run the Application
1. Clone the repository:
   ```bash
   git clone https://github.com/sajithdilhan/ECommerceBackend.git
   cd ECommerceBackend
   ```
2. Start the services using Docker Compose:
   ```bash
    docker-compose up --build -d
    ```
4. Restart User Api and Order Api
    ```bash
   docker compose restart user-api order-api
   ```
5. Access the services:
    - User Service: `http://localhost:5001` Swagger : http://localhost:5001/swagger/index.html
    - Order Service: `http://localhost:5002` Swagger : http://localhost:5002/swagger/index.html


## Resources 
- ChatGPT 
- Github Copilot
- Youtube

### AI prompts
- Use EF In-Memory Database in .NET core Web API.
- Implementation of a simple kafka producer and consumer in .NET Web API microservices. UserService and OrderService should both produce events and consume each others events.
- Extract a producer interface common to both consumers.
- Create a abstract base class for consumer classes with common methods.
- my project structure is both UserService and OrderService at root level same as docker compose. Each service has their own dockerfile. but when run docker compose up -d it gives me error. service "userservice" has neither an image nor a build context specified: invalid compose project.
- Im getting below error in my local setup. Confluent.Kafka.ConsumeException: 'Subscribed topic not available: order-created: Broker: Unknown topic or partition'
- Use global exception middleware in asp.net web api.


                            
