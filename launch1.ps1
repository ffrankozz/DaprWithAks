start-process powershell.exe -argument  '-command dapr run --app-id "order-service" --app-port "5001" --dapr-grpc-port "50010" --dapr-http-port "5010" --components-path "./components-azure" -- dotnet run --project ./OrdersApi/OrdersApi.csproj --urls="http://+:5001" '
start-process powershell.exe -argument  '-command dapr run --app-id "inventory-service" --app-port "5002" --dapr-grpc-port "50020" --dapr-http-port "5020" --components-path "./components-azure" -- dotnet run --project ./InventoryApi/InventoryApi.csproj --urls="http://+:5002" '
 
