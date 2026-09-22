# syntax=docker/dockerfile:1

FROM mikefarah/yq:4.53.6@sha256:cfc4eee658595834ef304eadb0c3ea721f3b7cb6404ad8b7cb909cc5b5145b23 AS yq

FROM mcr.microsoft.com/dotnet/sdk:8.0-noble

RUN apt-get update \
    && apt-get install --yes --no-install-recommends \
        bash \
        ca-certificates \
        coreutils \
        curl \
        gawk \
        grep \
        sed \
        unzip \
    && rm -rf /var/lib/apt/lists/*

COPY --from=yq /usr/bin/yq /usr/local/bin/yq

RUN yq --version

WORKDIR /workspace

CMD ["sleep", "infinity"]
