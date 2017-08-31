CREATE TABLE "order"."OrderEvent"
(
    "dbId" integer NOT NULL DEFAULT nextval('"order"."OrderEvent_dbId_seq"'::regclass),
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
