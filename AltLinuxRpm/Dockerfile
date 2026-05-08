FROM registry.altlinux.org/alt/base:p11

ENV LANG=ru_RU.UTF-8
ENV LC_ALL=ru_RU.UTF-8

RUN apt-get update -y && \
    apt-get install -y rpm-build && \
    apt-get clean

WORKDIR /workspace

