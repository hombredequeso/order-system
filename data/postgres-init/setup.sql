-- Database

CREATE DATABASE carrierpidgin
    WITH 
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.utf8'
    LC_CTYPE = 'en_US.utf8'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1;

\connect carrierpidgin

 -- Order Schema
CREATE SCHEMA "order"
    AUTHORIZATION postgres;

-- Order.OrderEvent
CREATE TABLE "order"."OrderEvent"
(
    "dbId" bigserial NOT NULL,
    "Id" uuid NOT NULL,
    "Version" bigint NOT NULL,
    "MessageType" character varying(100) COLLATE pg_catalog."default" NOT NULL,
    "SerializedMessage" json NOT NULL,
    "Timestamp" timestamp with time zone NOT NULL,
    CONSTRAINT "OrderEvent_pkey" PRIMARY KEY ("dbId"),
    CONSTRAINT "UniqueIdVersion" UNIQUE ("Id", "Version")
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE "order"."OrderEvent"
    OWNER to postgres;


-- Statistics Schema

CREATE SCHEMA "statistics"
    AUTHORIZATION postgres;

-- statistics.OrderStatistics table

CREATE TABLE "statistics"."OrderStatistics"
(
    "Id" uuid NOT NULL,
    "TotalOrders" integer NOT NULL,
    "Version" bigint NOT NULL,
    "UpdatedTimestamp" timestamp with time zone NOT NULL,
    CONSTRAINT "OrderStatistics_pkey" PRIMARY KEY ("Id")
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE "statistics"."OrderStatistics"
    OWNER to postgres;

--  statistics.MessageQueueProcessingDetails (for message de-dup.)

CREATE TABLE "statistics"."MessageQueueProcessingDetails"
(
    "Id" uuid NOT NULL,
--    "QueueName" character varying(100) COLLATE pg_catalog."default" NOT NULL,
    "QueueName" text NOT NULL,
    "LastMessageNumber" bigint NOT NULL,
    "Version" bigint NOT NULL,
    "UpdatedTimestamp" timestamp with time zone NOT NULL,
    CONSTRAINT "MessageQueueProcessingDetails_Statistics_pkey" PRIMARY KEY ("Id")
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE "statistics"."MessageQueueProcessingDetails"
    OWNER to postgres;

