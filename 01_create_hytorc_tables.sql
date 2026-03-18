-- =========================================================
-- HYTORC schema setup (sanitized version for portfolio/demo)
-- Replace SCHEMA_NAME with your own Oracle schema if needed.
-- You may also remove the SCHEMA_NAME prefix entirely and
-- create everything in your current schema.
-- =========================================================


-- =========================================================
-- 1) ADMINISTRATION
-- =========================================================
CREATE TABLE SCHEMA_NAME.EPE_UUM_HYTORC_ADMINISTRATION
(
    ID_ADMIN    NUMBER NOT NULL,
    SSO_ADMIN   NUMBER,
    NOM         VARCHAR2(100 BYTE),
    CONSTRAINT EPE_UUM_HYTORC_ADMINISTRATION_PK PRIMARY KEY (ID_ADMIN)
);


-- =========================================================
-- 2) TYPE_CLE
-- =========================================================
CREATE TABLE SCHEMA_NAME.EPE_UUM_HYTORC_TYPE_CLE
(
    TYPE        VARCHAR2(200 BYTE),
    ID_TYPE     NUMBER NOT NULL,
    CONSTRAINT EPE_UUM_HYTORC_TYPE_CLE_PK PRIMARY KEY (ID_TYPE)
);


-- =========================================================
-- 3) CLE
-- Depends on: TYPE_CLE
-- =========================================================
CREATE TABLE SCHEMA_NAME.EPE_UUM_HYTORC_CLE
(
    ID_CLE               NUMBER NOT NULL,
    NUMERO               VARCHAR2(100 BYTE),
    NUMERO_FOURNISSEUR   VARCHAR2(100 BYTE),
    DATE_DE_CONTROL      DATE,
    TYPE_ID              NUMBER,
    NOMBRE_UTILISATION   NUMBER DEFAULT 0,
    ETAT                 VARCHAR2(200 BYTE),
    CONSTRAINT EPE_UUM_HYTORC_CLE_PK PRIMARY KEY (ID_CLE),
    CONSTRAINT EPE_UUM_HYTORC_CLE_FK2 FOREIGN KEY (TYPE_ID)
        REFERENCES SCHEMA_NAME.EPE_UUM_HYTORC_TYPE_CLE (ID_TYPE)
);


-- =========================================================
-- 4) COUPLES_PRESSION
-- Depends on: CLE
-- =========================================================
CREATE TABLE SCHEMA_NAME.EPE_UUM_HYTORC_COUPLES_PRESSION
(
    ID_COUPLE_PRESSION   NUMBER NOT NULL,
    PRESSION             NUMBER,
    COUPLE               NUMBER,
    CLE_ID               NUMBER,
    CONSTRAINT EPE_UUM_HYTORC_COUPLES_PRESSION_PK PRIMARY KEY (ID_COUPLE_PRESSION),
    CONSTRAINT EPE_UUM_HYTORC_COUPLES_PRESSION_FK1 FOREIGN KEY (CLE_ID)
        REFERENCES SCHEMA_NAME.EPE_UUM_HYTORC_CLE (ID_CLE)
);


-- =========================================================
-- 5) MODIFICATION_CLE
-- Depends on: ADMINISTRATION, CLE
-- =========================================================
CREATE TABLE SCHEMA_NAME.EPE_UUM_HYTORC_MODIFICATION_CLE
(
    ID_MODIFICATION       NUMBER NOT NULL,
    DATE_DE_MISE_A_JOUR   DATE,
    ID_ADMINISTRATEUR     NUMBER,
    ACTION                VARCHAR2(200 BYTE),
    ID_CLE_MODIFIER       NUMBER,
    CONSTRAINT EPP_UMM_HYTORC_MODIFICATION_CLE_PK PRIMARY KEY (ID_MODIFICATION),
    CONSTRAINT EPE_UUM_HYTORC_MODIFICATION_CLE_FK1 FOREIGN KEY (ID_ADMINISTRATEUR)
        REFERENCES SCHEMA_NAME.EPE_UUM_HYTORC_ADMINISTRATION (ID_ADMIN),
    CONSTRAINT EPE_UUM_HYTORC_MODIFICATION_CLE_FK2 FOREIGN KEY (ID_CLE_MODIFIER)
        REFERENCES SCHEMA_NAME.EPE_UUM_HYTORC_CLE (ID_CLE)
);