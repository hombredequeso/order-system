# Prototype: http message bus example

A project for demonstrating a very simple message bus over http transport. As such, this is not a framework/library to be used in production. It is primarily for example/educational purposes.

The design is loosely based on Vaughn Vernon's example in _Implementing Domain-Driven Design_, chapter 8.

### Why Pidgin?
No it's not misspelt. Originally I was going to call it carrier-pigeon, because it, you know, carries messages around.
But the [fount of wisdom](https://en.wikipedia.org/wiki/Pidgin) has this to say about pidgin:
"is a grammatically simplified means of communication that develops between two or more groups that do not have a language in common: typically, a mixture of simplified languages or a simplified primary language with other languages' elements included."

And that language is http.

## Getting Started

### Prerequisites

Something to compile/run a simple dot net application with.

### Postgres Database

Some services use a postgres database.
The simplest way to set this up is likely to be using docker.
For example:


Start postgres database, exposed on port 5432 on the local/host machine (note: this will not persist between restarts)

```shell
docker run --name some-postgres -e POSTGRES_PASSWORD=mypassword -p 5432:5432 -d postgres
```

Then run scripts to setup the database

```shell
cd data\postgres-init
docker cp .\setup.sql some-postgres:/home/setup.sql
docker exec -it some-postgres psql -U postgres -f /home/setup.sql
```

### Postman

The file data/carrier-pidgin.postman_collection.json contains a sample collection of sample api requests that can be loaded in to [Postman](https://www.getpostman.com/).

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

