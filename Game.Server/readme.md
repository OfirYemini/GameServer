in order to run the application

1. install dotnet-ef tool: 
dotnet tool install --global dotnet-ef --version 8.0.0
2. cd into GameServer project and run:
dotnet ef migrations add InitialCreate
3. dotnet ef database update     

if you want to drop the current db:
dotnet ef database drop   


todo:
1. add tests
2. remove todos
2. clean architecture
2. check functionaliy V
3. enrich data with playerId
4. support scaling
5. add integration tests with test containers
6. dockerize server and client
7. validate db schema V
8. handle validations in a clean way
9. add polly policy