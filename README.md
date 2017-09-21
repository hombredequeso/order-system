# Eductional project for multiple domains over an http bus

This is a project intended for eductional purposes. It is directed towards anyone wanting to see an example of a messaging framework with a contrived domain example, primarily in C#.

For documentation about various parts of the project and the patterns used, see the project [wiki](https://github.com/hombredequeso/carrier-pidgin/wiki).

The project has two main parts:
* Implementation of an http based bus for interdomain communication.
* Simple (contrived!) domain examples (I'm the product owner, so I can make it do what-ever I want, and that's exactly what it does)

The http bus design is based on Vaughn Vernon's example in _Implementing Domain-Driven Design_, chapter 8.

### Setting the Scene

Aside from a few low-level libraries concerned with such things as serialization, database access, and unit testing, there is very little in the way of libraries. There is no Ioc container. There is nothing above the low-level database access, so sql commands must be written. This is not because higher-level libraries are evil -- personally I suggest you use them -- but to illustrate as explicitly as possible the complete workings of the system.

Is this object-oriented, or functional? Probably this project is a bit split brain. I'd rather be writing functional, but it started with C# because that is what the eductional situation initially demanded. For anyone familiar with functional programming it is probably clear that my C# is pining to be truly functional. (and whether you agree or not, one of the lower-level libraries I use is a C# option library. Surely 'option' isn't a higher level construct is it?).

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

The PostgreSQL server can be set up using the docker-compose.yml file provided:

```shell
docker-compose up
```

To create the database itself:

```shell
docker exec -it carrier-pidgin-postgres psql -U postgres -f /home/data/postgres-init/setup.sql
```

The database will only be created, or destroyed and recreated from scratch, when the docker exec command is run.
This means it is possible to restart the database and keep it in the state it was last time it was used.
This is suitable for development purposes only.

### Postman

The file data/carrier-pidgin.postman_collection.json contains a sample collection of sample api requests that can be loaded in to [Postman](https://www.getpostman.com/).

## Getting Started with Postgres on Windows
There are a variety of ways to get a dev postgres setup going. The following is what I use, with basic installation and getting started instructions.
* Run the postgres database server using docker/docker-compose, as above.
* For a GUI database administration client, download and install [pgadmin](https://www.pgadmin.org/download/pgadmin-4-windows/). Assuming you have used any other UI based database admin clients, this should be pretty familiar.
* In the WSL (Windows subsystem for linux) install psql, a terminal based PostgreSQL client, as follows:

```shell
sudo apt-get update
sudo apt-get install postgresql postgresql-contrib
psql -p 5432 -h localhost -U postgres
```

Once in psql, various psql (administration) commands and sql commands can be run. Just don't forget the semi-colon at the end of a sql commands.
e.g.
To list the databases:

```shell
postgres=# \l
```

To delete a database:

```shell
DROP DATABASE carrierpidgin;
```

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

