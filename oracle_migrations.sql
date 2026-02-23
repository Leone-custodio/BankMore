BEGIN
   EXECUTE IMMEDIATE 'DROP TABLE tarifa CASCADE CONSTRAINTS';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
   EXECUTE IMMEDIATE 'DROP TABLE transferencia CASCADE CONSTRAINTS';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
   EXECUTE IMMEDIATE 'DROP TABLE movimento CASCADE CONSTRAINTS';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
   EXECUTE IMMEDIATE 'DROP TABLE contacorrente CASCADE CONSTRAINTS';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
   EXECUTE IMMEDIATE 'DROP TABLE agencia CASCADE CONSTRAINTS';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
   EXECUTE IMMEDIATE 'DROP TABLE usuario CASCADE CONSTRAINTS';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/
BEGIN
   EXECUTE IMMEDIATE 'DROP TABLE idempotencia CASCADE CONSTRAINTS';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/

CREATE TABLE usuario (
    idusuario VARCHAR2(50) PRIMARY KEY,
    nome VARCHAR2(100) NOT NULL,
    cpf VARCHAR2(11) NOT NULL UNIQUE,
    celular VARCHAR2(20) NOT NULL,
    endereco VARCHAR2(200) NOT NULL,
    senha VARCHAR2(100) NOT NULL,
    salt VARCHAR2(100) NOT NULL
);

CREATE TABLE agencia (
    idagencia VARCHAR2(50) PRIMARY KEY,
    numero VARCHAR2(10) NOT NULL UNIQUE,
    nome VARCHAR2(100) NOT NULL
);

CREATE TABLE contacorrente (
    idcontacorrente VARCHAR2(50) PRIMARY KEY,
    idusuario VARCHAR2(50) NOT NULL,
    idagencia VARCHAR2(50) NOT NULL,
    numero NUMBER(10) NOT NULL UNIQUE,
    nome VARCHAR2(100) NOT NULL,
    ativo NUMBER(1) DEFAULT 1 NOT NULL,
    saldo NUMBER(15,2) DEFAULT 0 NOT NULL,
    CONSTRAINT check_ativo CHECK (ativo IN (0, 1)),
    CONSTRAINT fk_conta_usuario FOREIGN KEY (idusuario) REFERENCES usuario(idusuario),
    CONSTRAINT fk_conta_agencia FOREIGN KEY (idagencia) REFERENCES agencia(idagencia)
);

CREATE TABLE idempotencia (
    chave_idempotencia VARCHAR2(50) PRIMARY KEY,
    requisicao CLOB,
    resultado CLOB
);

CREATE TABLE movimento (
    idmovimento VARCHAR2(50) PRIMARY KEY,
    idcontacorrente VARCHAR2(50) NOT NULL,
    datamovimento VARCHAR2(25) NOT NULL,
    tipomovimento CHAR(1) NOT NULL,
    valor NUMBER(18, 2) NOT NULL,
    CONSTRAINT check_tipo_mov CHECK (tipomovimento IN ('C', 'D')),
    CONSTRAINT fk_mov_conta FOREIGN KEY (idcontacorrente) REFERENCES contacorrente(idcontacorrente)
);

CREATE TABLE transferencia (
    idtransferencia VARCHAR2(50) PRIMARY KEY,
    idcontacorrente_origem VARCHAR2(50) NOT NULL,
    idcontacorrente_destino VARCHAR2(50) NOT NULL,
    datamovimento VARCHAR2(25) NOT NULL,
    valor NUMBER(18, 2) NOT NULL,
    CONSTRAINT fk_transf_origem FOREIGN KEY (idcontacorrente_origem) REFERENCES contacorrente(idcontacorrente),
    CONSTRAINT fk_transf_destino FOREIGN KEY (idcontacorrente_destino) REFERENCES contacorrente(idcontacorrente)
);

CREATE TABLE tarifa (
    idtarifa VARCHAR2(50) PRIMARY KEY,
    idcontacorrente VARCHAR2(50) NOT NULL,
    datamovimento VARCHAR2(25) NOT NULL,
    valor NUMBER(18, 2) NOT NULL,
    CONSTRAINT fk_tarifa_conta FOREIGN KEY (idcontacorrente) REFERENCES contacorrente(idcontacorrente)
);

COMMIT;
