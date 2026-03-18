# Hytorc Management Application (C# / Oracle)

## Overview
This project is a C# Windows Forms application developed to manage Hytorc tools in an industrial environment.

It provides features for:
- Managing tools (add, activate, deactivate)
- Processing calibration files and updating the database
- Calculating pressure based on torque values
- Tracking tool usage
- Generating Excel reports
- Logging user actions (audit trail)

---

## Technologies Used
- C# (.NET Framework)
- Windows Forms
- Oracle Database
- Oracle Managed Data Access
- Active Directory (LDAP authentication)
- ClosedXML (Excel generation)

---

## Configuration

### App.config

```xml
<appSettings>
  <add key="ArchiveFolder" value="YOUR_ARCHIVE_PATH"/>
  <add key="InputFolder" value="YOUR_INPUT_PATH"/>
</appSettings>
```

## Database Setup

Run the SQL script:
```
database/01_create_hytorc_tables.sql
```
This script:

- creates all required tables

- defines primary and foreign keys

- is sanitized for public use

If the script contains SCHEMA_NAME, you can:

- replace it with your Oracle schema

- or remove it to use your default schema

## Oracle Connection

The application connects to Oracle using Oracle Managed Data Access.

Connection settings are read from an external file:
```
connect_securedb.db
```
Example format:
```
Data Source=HOST:PORT/SERVICE_NAME;
User Id=USERNAME;
Password=PASSWORD;
```
This file is not included in the repository for security reasons.

## Database Access & SQL

The application uses a dedicated class: Connect_db.

Main features:

- centralized connection management

- parameterized SQL queries

- separation between SELECT and UPDATE operations

Example queries

SELECT:
```
SELECT * FROM EPE_UUM_HYTORC_CLE WHERE NUMERO = :num
```
UPDATE:
```
UPDATE EPE_UUM_HYTORC_CLE 
SET ETAT = 'ACTIVE' 
WHERE NUMERO = :num
```
COUNT:
```
SELECT COUNT(*) FROM EPE_UUM_HYTORC_CLE WHERE NUMERO = :num
```
## Good practices

- use of parameters (:param)

- no raw SQL concatenation

- controlled connection handling

- error logging

- File Processing

The application processes calibration files:

- reads text files

- extracts data

- updates database

- archives processed files

- Excel Export

Excel files are generated using ClosedXML:

# tool list

metadata (type, supplier, date, status)

## Authentication

Uses Active Directory (LDAP):
```
DirectoryEntry("LDAP://domain", username, password)
```
This feature requires a corporate environment.

## Limitations

- This project was originally developed in an enterprise context:

- requires Oracle database

- uses LDAP authentication

- depends on external configuration

- This version is adapted for demonstration purposes.

## Key Features

- Oracle database integration

- secure SQL queries (parameterized)

- file processing automation

- audit logging system

- configurable application
