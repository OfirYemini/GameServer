in order to run the application

1. install dotnet-ef tool: 
dotnet tool install --global dotnet-ef --version 8.0.0
2. cd into GameServer project and run:
dotnet ef migrations add InitialCreate
3. dotnet ef database update     

if you want to drop the current db:
dotnet ef database drop   

solution is structured according to clean architecture

todo:
1. add tests
2. clean architecture V
3. check functionaliy V
4. support scaling
5. add integration tests with test containers
6. dockerize server and client
7. validate db schema V
8. handle validations in a clean way
9. add polly policy