in order to run the application

1. install dotnet-ef tool: 
dotnet tool install --global dotnet-ef --version 8.0.0
2. cd into GameServer project and run:
dotnet ef migrations add InitialCreate
3. dotnet ef database update     

if you want to drop the current db:
dotnet ef database drop   

configuration can be set for multiple providers (appsettings, env vars, args) according to best practices
solution is structured according to clean architecture

on game repo I use transaction for transfer operation to avoid partial success
on game repo I use concurrency version for optimistic locking to avoid incossitency in data as a result from parallel exeutions
the login action will add a player if deviceId is new, Idealy we would have a seperate signin method but for the sake of testing simpliciy I will keep the current behavior


todo:
1. add tests
2. clean architecture V
3. check functionaliy V
4. support scaling
5. add integration tests with test containers
6. dockerize server and client
7. validate db schema V
8. handle validations in a clean way V
9. fix client console print 
10. allow client extension
