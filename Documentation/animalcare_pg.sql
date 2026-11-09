-- ============================================================
-- 0. Create DB  (run while connected to "postgres")
-- ============================================================
-- DROP DATABASE IF EXISTS animalcaredb;
-- CREATE DATABASE animalcaredb;
-- -- \c animalcaredb

-- ============================================================
-- 1. Drop (reverse dependency order)
-- ============================================================
DROP TABLE IF EXISTS notification       CASCADE;
DROP TABLE IF EXISTS medicalhistory     CASCADE;
DROP TABLE IF EXISTS appointment        CASCADE;
DROP TABLE IF EXISTS schedule           CASCADE;
DROP TABLE IF EXISTS animal             CASCADE;
DROP TABLE IF EXISTS veterinarian       CASCADE;
DROP TABLE IF EXISTS owner              CASCADE;
DROP TABLE IF EXISTS useraccount        CASCADE;
DROP TABLE IF EXISTS role               CASCADE;

DROP VIEW  IF EXISTS dailyagenda        CASCADE;
DROP VIEW  IF EXISTS monthlyreport      CASCADE;

DROP FUNCTION IF EXISTS fn_preventdoublebooking()        CASCADE;
DROP FUNCTION IF EXISTS fn_updateschedulestatus()        CASCADE;
DROP FUNCTION IF EXISTS fn_addremindernotification()     CASCADE;

-- ============================================================
-- 2. Tables
-- ============================================================
CREATE TABLE role (
    roleid    SERIAL PRIMARY KEY,
    rolename  VARCHAR(50) NOT NULL
);

CREATE TABLE useraccount (
    userid         SERIAL PRIMARY KEY,
    userlogin      VARCHAR(100) UNIQUE NOT NULL,
    userpassword   VARCHAR(255) NOT NULL,
    userfirstname  VARCHAR(50),
    userlastname   VARCHAR(50),
    roleid         INT NOT NULL REFERENCES role(roleid),
    useractive     BOOLEAN DEFAULT TRUE
);

CREATE TABLE owner (
    ownerid        SERIAL PRIMARY KEY,
    ownerlastname  VARCHAR(50),
    ownerfirstname VARCHAR(50),
    ownerphone     VARCHAR(20),
    owneremail     VARCHAR(100),
    owneraddress   VARCHAR(255)
);

CREATE TABLE veterinarian (
    vetid          SERIAL PRIMARY KEY,
    vetlastname    VARCHAR(50),
    vetfirstname   VARCHAR(50),
    vetspecialty   VARCHAR(100),
    vetphone       VARCHAR(20),
    vetemail       VARCHAR(100)
);

CREATE TABLE animal (
    animalid      SERIAL PRIMARY KEY,
    animalname    VARCHAR(50),
    species       VARCHAR(50),
    breed         VARCHAR(50),
    sex           CHAR(1),
    birthdate     DATE,
    animalweight  NUMERIC(5,2),
    ownerid       INT NOT NULL,
    CONSTRAINT fk_animal_owner FOREIGN KEY (ownerid)
        REFERENCES owner(ownerid) ON DELETE CASCADE
);

CREATE TABLE schedule (
    scheduleid      SERIAL PRIMARY KEY,
    vetid           INT NOT NULL,
    scheduledate    DATE NOT NULL,
    starttime       TIME NOT NULL,
    endtime         TIME NOT NULL,
    schedulestatus  VARCHAR(10)
        CHECK (schedulestatus IN ('Free','Busy')),
    CONSTRAINT fk_schedule_vet FOREIGN KEY (vetid)
        REFERENCES veterinarian(vetid) ON DELETE CASCADE
);

CREATE TABLE appointment (
    appointmentid      SERIAL PRIMARY KEY,
    vetid              INT NOT NULL,
    animalid           INT NOT NULL,
    startdatetime      TIMESTAMP NOT NULL,
    enddatetime        TIMESTAMP NOT NULL,
    purpose            VARCHAR(255),
    appointmentstatus  VARCHAR(15)
        CHECK (appointmentstatus IN ('Planned','Canceled','Completed')),
    CONSTRAINT fk_appointment_vet FOREIGN KEY (vetid)
        REFERENCES veterinarian(vetid),
    CONSTRAINT fk_appointment_animal FOREIGN KEY (animalid)
        REFERENCES animal(animalid) ON DELETE CASCADE
);

CREATE TABLE medicalhistory (
    historyid         SERIAL PRIMARY KEY,
    animalid          INT NOT NULL,
    consultationdate  DATE NOT NULL,
    observations      TEXT,
    diagnosis         TEXT,
    treatment         TEXT,
    vetid             INT NOT NULL,
    CONSTRAINT fk_medicalhistory_animal FOREIGN KEY (animalid)
        REFERENCES animal(animalid) ON DELETE CASCADE,
    CONSTRAINT fk_medicalhistory_vet FOREIGN KEY (vetid)
        REFERENCES veterinarian(vetid)
);

CREATE TABLE notification (
    notificationid      SERIAL PRIMARY KEY,
    appointmentid       INT NOT NULL,
    notificationtype    VARCHAR(20)
        CHECK (notificationtype IN ('Reminder','Cancellation','Change','Completed','Updated')),
    senddate            TIMESTAMP,
    channel             VARCHAR(10) CHECK (channel IN ('Email','SMS')),
    notificationstatus  VARCHAR(10) CHECK (notificationstatus IN ('Sent','Pending')),
    CONSTRAINT fk_notification_appointment FOREIGN KEY (appointmentid)
        REFERENCES appointment(appointmentid) ON DELETE CASCADE
);

-- ============================================================
-- 3. Indexes  (match the ones in AnimalCareContext.OnModelCreating)
-- ============================================================
CREATE INDEX ix_useraccount_roleid      ON useraccount(roleid);
CREATE INDEX ix_animal_ownerid          ON animal(ownerid);
CREATE INDEX ix_schedule_vetid          ON schedule(vetid);
CREATE INDEX ix_appointment_vetid       ON appointment(vetid);
CREATE INDEX ix_appointment_animalid    ON appointment(animalid);
CREATE INDEX ix_appointment_startdt     ON appointment(startdatetime);
CREATE INDEX ix_medicalhistory_animalid ON medicalhistory(animalid);
CREATE INDEX ix_medicalhistory_vetid    ON medicalhistory(vetid);
CREATE INDEX ix_notification_appt       ON notification(appointmentid);

-- ============================================================
-- 4. Sample data
-- ============================================================
INSERT INTO role (rolename) VALUES
 ('Administrator'), ('Veterinarian'), ('Receptionist');

INSERT INTO useraccount (userlogin,userpassword,userfirstname,userlastname,roleid,useractive) VALUES
 ('admin2',       'Admin2@123',        'Alvaro', 'Limaymanta',         1, TRUE),
 ('vetMaria',         'VetMaria@123',          'Maria',   'Gonzales', 2, TRUE),
 ('recepLucas','RecepLucas@123', 'Lucas',  'George', 3, TRUE);

INSERT INTO owner (ownerlastname,ownerfirstname,ownerphone,owneremail,owneraddress) VALUES
 ('Juan','Perez','514-123-4567','perez@email.com','123 Saint Catherine, Montreal'),
 ('Luan','Tremblay','514-234-5678','tremblay','456 Sherbrooke, Montreal');

INSERT INTO veterinarian (vetlastname,vetfirstname,vetspecialty,vetphone,vetemail) VALUES
 ('Johnson','Emily','Surgery','514-098-9876','emily@animalcare.com'),
 ('Williams','Michael','General Care','514-098-9877','michael@animalcare.com');

INSERT INTO schedule (vetid,scheduledate,starttime,endtime,schedulestatus) VALUES
 (1,'2025-12-01','09:00','17:00','Free'),
 (2,'2025-12-01','09:00','17:00','Free');

INSERT INTO animal (animalname,species,breed,sex,birthdate,animalweight,ownerid) VALUES
 ('Bella','Dog','Golden Retriever','F','2020-05-10',28.5,1),
 ('Max','Cat','Persian','M','2021-03-15',5.2,2);

-- NOTE: the AFTER INSERT triggers will populate Schedule + Notification rows,
-- so insert appointments last.
INSERT INTO appointment (vetid,animalid,startdatetime,enddatetime,purpose,appointmentstatus) VALUES
 (1,1,'2025-12-01 10:00','2025-12-01 10:30','Routine Checkup','Planned'),
 (2,2,'2025-12-01 14:00','2025-12-01 14:30','Vaccination','Planned');

INSERT INTO medicalhistory (animalid,consultationdate,observations,diagnosis,treatment,vetid) VALUES
 (1,'2025-11-01','Healthy, slight weight gain','None','Diet Adjustment',1),
 (2,'2025-11-15','Healthy cat, regular checkup','None','Vaccination booster',2);

-- ============================================================
-- 5. Views  (monthlyreport + dailyagenda)
-- ============================================================
CREATE OR REPLACE VIEW monthlyreport AS
SELECT
    EXTRACT(MONTH FROM a.startdatetime)::INT AS month,
    EXTRACT(YEAR  FROM a.startdatetime)::INT AS year,
    COUNT(a.appointmentid) AS numberofappointments,
    SUM(CASE WHEN a.appointmentstatus = 'Canceled' THEN 1 ELSE 0 END) AS numbercanceled,
    COALESCE(
        (SELECT COUNT(DISTINCT v.vetid)
         FROM veterinarian v
         JOIN schedule s ON v.vetid = s.vetid
         WHERE s.schedulestatus IN ('Free','Busy')), 0
    ) AS numberofactivevets,
    COALESCE(
        ROUND(
            SUM(CASE WHEN a.appointmentstatus IN ('Planned','Completed') THEN 1 ELSE 0 END) * 100.0
            / NULLIF((SELECT COUNT(*) FROM schedule s WHERE s.schedulestatus IN ('Free','Busy')),0)
        , 2), 0
    ) AS occupancyrate
FROM appointment a
GROUP BY EXTRACT(YEAR FROM a.startdatetime), EXTRACT(MONTH FROM a.startdatetime);

CREATE OR REPLACE VIEW dailyagenda AS
SELECT
    a.startdatetime::date                       AS date,
    v.vetlastname || ' ' || v.vetfirstname      AS veterinarianname,
    a.startdatetime::time                       AS starttime,
    a.enddatetime::time                         AS endtime,
    an.animalname,
    o.ownerlastname || ' ' || o.ownerfirstname  AS ownername,
    a.appointmentstatus
FROM appointment a
JOIN veterinarian v ON a.vetid   = v.vetid
JOIN animal       an ON a.animalid = an.animalid
JOIN owner         o ON an.ownerid = o.ownerid;

-- ============================================================
-- 6. Triggers  (must be declared BEFORE first appointment insert
--    for the sample data to populate schedule + notifications)
-- ============================================================

-- 6a. prevent double-booking
CREATE OR REPLACE FUNCTION fn_preventdoublebooking() RETURNS trigger AS $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM appointment a
        JOIN new_rows i ON a.vetid = i.vetid
        WHERE a.appointmentid <> i.appointmentid
          AND a.appointmentstatus <> 'Canceled'
          AND i.startdatetime < a.enddatetime
          AND i.enddatetime   > a.startdatetime
    ) THEN
        RAISE EXCEPTION 'Error: Veterinarian already booked for this time slot.';
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_preventdoublebooking
AFTER INSERT ON appointment
REFERENCING NEW TABLE AS new_rows
FOR EACH STATEMENT
EXECUTE FUNCTION fn_preventdoublebooking();

-- 6b. update schedule status
CREATE OR REPLACE FUNCTION fn_updateschedulestatus() RETURNS trigger AS $$
BEGIN
    UPDATE schedule s
       SET schedulestatus = 'Busy'
      FROM new_rows i
     WHERE s.vetid = i.vetid
       AND s.scheduledate = i.startdatetime::date;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_updateschedulestatus
AFTER INSERT ON appointment
REFERENCING NEW TABLE AS new_rows
FOR EACH STATEMENT
EXECUTE FUNCTION fn_updateschedulestatus();

-- 6c. add reminder notification
CREATE OR REPLACE FUNCTION fn_addremindernotification() RETURNS trigger AS $$
BEGIN
    INSERT INTO notification
        (appointmentid, notificationtype, senddate, channel, notificationstatus)
    SELECT i.appointmentid,
           'Reminder',
           i.startdatetime - INTERVAL '1 day',
           'Email',
           'Pending'
    FROM new_rows i;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_addremindernotification
AFTER INSERT ON appointment
REFERENCING NEW TABLE AS new_rows
FOR EACH STATEMENT
EXECUTE FUNCTION fn_addremindernotification();