# .NET Cloud native app sample

.NET app running on Docker and Kubernetes. 

## Docker Compose

To run with ```docker-compose.yaml```:

```sh
docker compose up
```

To run with ```docker-compose.yaml.debug```:

```sh
docker compose -f docker-compose.debug.yml up
```

Add ``-d`` to run in detached mode.

Add ``--build`` to rebuild images if the projects have changed.

Launch your browser and hit: http://localhost:8010/swagger

## Kubernetes (Minikube)

Assuming you are using Minikube, and that it already has been configured. 

You will need to build and put the image in Minikube internal registry. More on how to setup that below.

### Short version

If all is set-up correctly, simply apply the ``.yaml`` files found ``k8s`` directory - in s pretty logical order:

```sh
kubectl apply -f mssql.yaml
kubectl apply -f rabbitmq.yaml
kubectl apply -f zipkin.yaml
kubectl apply -f webapplication2.yaml
kubectl apply -f worker.yaml
kubectl apply -f loki.yaml
kubectl apply -f prometheus.yaml
kubectl apply -f grafana.yaml
```

To delete the deployments:

```sh
kubectl delete -f webapplication2.yaml
kubectl delete -f worker.yaml
```

To then expose the main "webbapplication2" app to the host:

```sh
minikube service webapplication2 --url
```

That will return a URL for that service. The same can be done for other services, like Zipkin.

You can now hit: ``<webapplication2 URL>/swagger``

### Build, tag, and push image to registry

When working against the Minikube internal registry, all you need to do is building the images.

```sh
docker build . -f WebApplication2/Dockerfile -t sundis/webapplication2:latest
docker build . -f Worker/Dockerfile -t sundis/worker:latest
```

Replace the relevant values with your own.

### How to set up Minikube internal registry

Minikube has its own internal registry.

You can get the details on how to connect to the Minikube registry from running this command:

```sh
minikube docker-env
```

It will output something similar to this:

```
export DOCKER_TLS_VERIFY=”1"
export DOCKER_HOST=”tcp://172.17.0.2:2376"
export DOCKER_CERT_PATH=”/home/user/.minikube/certs”
export MINIKUBE_ACTIVE_DOCKERD=”minikube”
# To point your shell to minikube’s docker-daemon, run:
# eval $(minikube -p minikube docker-env)
```

And apply those connection settings to the Docker daemon.

```sh
eval $(minikube -p minikube docker-env)
```

Read more here: https://medium.com/swlh/how-to-run-locally-built-docker-images-in-kubernetes-b28fbc32cc1d 

### Mount directory in Minikube

Expose a directory in the host system to Minikube.

```sh
minikube mount "$HOME:/host" --uid=10001
```

Example:

```sh
minikube mount "/Users/marina/Projects/WebApplication2/.data:/host"
minikube mount "/Users/marina/Projects/WebApplication2/grafana:/grafana"
minikube mount "/Users/marina/ProjectsWebApplication2/prometheus:/prometheus"
```