# deploy Dapr components
kubectl apply -f .\deploy\component-pubsub-asb.yaml
kubectl apply -f .\deploy\component-binding-eh.yaml
kubectl apply -f .\deploy\component-orderstore-cosmos.yaml
kubectl apply -f .\deploy\component-inventorystore-cosmos.yaml
kubectl apply -f .\deploy\component-inventoryitemstore-cosmos.yaml
 

# deploy Dapr microservices (deployment and service file in the same deployment file)
kubectl apply -f .\deploy\microservice.order.yaml
kubectl apply -f .\deploy\microservice.inventory.yaml
 