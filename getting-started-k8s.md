# Getting started (Kubernetes)

Steps for running locally in Kubernetes on Minikube.

The commands are for shells in *NIX environments, like Linux and macOS. These have been tested on a Mac. 

Most will work directly in Windows, others need to be translated in you are not using Bash as your shell.

## Install Docker Desktop

Download Docker from [here](https://www.docker.com/) and install.

Start Docker.

## Install Minikube

Download Minikube from [here](https://minikube.sigs.k8s.io/docs/start/). Follow the instructions.

Run this to start Minikube.

```sh
minikube start
```

This starts a Docker container running Minikube. It will also configure ``kubectl``.

### Enable Minikube Registry

Enable the ``registry`` plugin. Essentially an internal Docker Registry.

```sh
minikube addons enable registry
```

### Configure Docker Daemon to use Minikube Registry

Point the Docker Daemon to the Minikube internal registry.

```sh
eval $(minikube -p minikube docker-env)
```

This needs to be run in every new console Windows

### Mount directory at host in Minikube

In order to persist data outside of Kubernetes we need to map a volume. 

Minikube is a closed system, so you need to tell what it is allowed to access from the host. To access the host's filesystem from a pod we need to mount a path in Minikube.

This will mount the home folder at the host to ``/host`` in Minikube.

```sh
minikube mount $HOME:/host --uid=10001
```

You can change this location to whatever you like if you don't want data files to end up in your home folder.


## Run the projects

### Build images

```sh
docker build . -f WebApplication2/Dockerfile -t sundis/webapplication2:latest
docker build . -f Worker/Dockerfile -t sundis/worker:latest
```
Provided that you have configured the Docker daemon for Minikube Registry, the images will be stored in Minikube. That way Minikube can use them without having to push them to another registry,

### Deploy

Deploy services:

```sh
kubectl apply -f mssql.yaml
kubectl apply -f rabbitmq.yaml
kubectl apply -f zipkin.yaml
kubectl apply -f worker.yaml
kubectl apply -f webapplication2.yaml
```

Delete the deployments:

```sh
kubectl delete -f mssql.yaml
kubectl delete -f rabbitmq.yaml
kubectl delete -f zipkin.yaml
kubectl delete -f worker.yaml
kubectl delete -f webapplication2.yaml
```

### Expose service to host 

This will expose the ``webapplication2`` service to the host, and open up the browser:

```sh
minikube service webapplication2
```

Substitute ``webapplication2`` with the name of the service that you want to access from the host.

## Remember

You need to:
* Mount the paths used by pods to store data files in. Such as for SQL Server.

* Configure the Docker daemon in every Terminal/Console session if your want interact with the Minikube registry from the host. Like when building images.
