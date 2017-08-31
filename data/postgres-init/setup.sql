CREATE DATABASE carrierpidgin
    WITH 
    OWNER = postgres
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.utf8'
    LC_CTYPE = 'en_US.utf8'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1;

\connect carrierpidgin

CREATE SCHEMA "order"
    AUTHORIZATION postgres;

CREATE TABLE "order"."OrderEvent"
(
    "dbId" SERIAL NOT NULL,
    "Id" uuid NOT NULL,
    "Version" integer NOT NULL,
    "MessageType" character varying(100) COLLATE pg_catalog."default" NOT NULL,
    "SerializedMessage" json NOT NULL,
    "Timestamp" timestamp with time zone NOT NULL,
    CONSTRAINT "OrderEvent_pkey" PRIMARY KEY ("dbId")
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE "order"."OrderEvent"
    OWNER to postgres;

