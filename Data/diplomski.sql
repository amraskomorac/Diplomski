CREATE DATABASE IF NOT EXISTS diplomski
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE diplomski;

CREATE TABLE korisnici (
    id INT NOT NULL AUTO_INCREMENT,
    puno_ime VARCHAR(100) NOT NULL,
    email VARCHAR(150) NOT NULL,
    lozinka_hash VARCHAR(255) NOT NULL,
    datum_kreiranja DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT PK_korisnici PRIMARY KEY (id),
    CONSTRAINT UQ_korisnici_email UNIQUE (email)
);

CREATE TABLE tokeni_za_reset_lozinke (
    id INT NOT NULL AUTO_INCREMENT,
    korisnik_id INT NOT NULL,
    token VARCHAR(255) NOT NULL,
    istice DATETIME NOT NULL,
    iskoristen BOOLEAN NOT NULL DEFAULT FALSE,
    CONSTRAINT PK_tokeni_za_reset_lozinke PRIMARY KEY (id),
    CONSTRAINT FK_tokeni_za_reset_lozinke_korisnici_korisnik_id
        FOREIGN KEY (korisnik_id) REFERENCES korisnici (id) ON DELETE CASCADE
);

CREATE TABLE vozila (
    id INT NOT NULL AUTO_INCREMENT,
    marka VARCHAR(60) NOT NULL,
    model VARCHAR(60) NOT NULL,
    godina_proizvodnje INT NOT NULL,
    registracija VARCHAR(20) NOT NULL,
    tip_goriva VARCHAR(30) NOT NULL,
    tip_mjenjaca VARCHAR(30) NOT NULL,
    trenutna_kilometraza INT NOT NULL,
    datum_kupovine DATETIME NOT NULL,
    datum_registracije DATETIME NOT NULL,
    datum_isteka_registracije DATETIME(6) NULL,
    cijena_registracije DECIMAL(10, 2) NULL,
    polica_osiguranja VARCHAR(150) NULL,
    kilometraza_mali_servis INT NOT NULL DEFAULT 0,
    kilometraza_veliki_servis INT NOT NULL DEFAULT 0,
    korisnik_id INT NOT NULL,
    CONSTRAINT PK_vozila PRIMARY KEY (id),
    CONSTRAINT FK_vozila_korisnici_korisnik_id
        FOREIGN KEY (korisnik_id) REFERENCES korisnici (id) ON DELETE CASCADE
);

CREATE TABLE goriva (
    id INT NOT NULL AUTO_INCREMENT,
    vozilo_id INT NOT NULL,
    datum DATETIME NOT NULL,
    litara DECIMAL(10, 2) NOT NULL,
    cijena DECIMAL(10, 2) NOT NULL,
    kilometraza INT NOT NULL,
    korisnik_id INT NOT NULL,
    CONSTRAINT PK_goriva PRIMARY KEY (id),
    CONSTRAINT FK_goriva_vozila_vozilo_id
        FOREIGN KEY (vozilo_id) REFERENCES vozila (id) ON DELETE CASCADE,
    CONSTRAINT FK_goriva_korisnici_korisnik_id
        FOREIGN KEY (korisnik_id) REFERENCES korisnici (id) ON DELETE CASCADE
);

CREATE TABLE servisi (
    id INT NOT NULL AUTO_INCREMENT,
    tip VARCHAR(150) NOT NULL,
    datum DATETIME(6) NOT NULL,
    kilometraza INT NOT NULL,
    cijena DECIMAL(10, 2) NOT NULL,
    serviser VARCHAR(150) NOT NULL,
    napomena VARCHAR(2000) NULL,
    putanja_racuna VARCHAR(500) NULL,
    vozilo_id INT NOT NULL,
    korisnik_id INT NOT NULL,
    CONSTRAINT PK_servisi PRIMARY KEY (id),
    CONSTRAINT FK_servisi_vozila_vozilo_id
        FOREIGN KEY (vozilo_id) REFERENCES vozila (id) ON DELETE CASCADE,
    CONSTRAINT FK_servisi_korisnici_korisnik_id
        FOREIGN KEY (korisnik_id) REFERENCES korisnici (id) ON DELETE CASCADE
);

CREATE TABLE historija_registracija (
    id INT NOT NULL AUTO_INCREMENT,
    vozilo_id INT NOT NULL,
    datum_registracije DATETIME(6) NOT NULL,
    datum_isteka_registracije DATETIME(6) NOT NULL,
    cijena DECIMAL(10, 2) NULL,
    polica_osiguranja VARCHAR(150) NULL,
    CONSTRAINT PK_historija_registracija PRIMARY KEY (id),
    CONSTRAINT FK_historija_registracija_vozila_vozilo_id
        FOREIGN KEY (vozilo_id) REFERENCES vozila (id) ON DELETE CASCADE
);

CREATE TABLE dokumenti (
    id INT NOT NULL AUTO_INCREMENT,
    tip VARCHAR(50) NOT NULL,
    naziv_datoteke VARCHAR(255) NOT NULL,
    putanja VARCHAR(500) NOT NULL,
    datum_izmjene DATETIME NOT NULL,
    korisnik_id INT NOT NULL,
    CONSTRAINT PK_dokumenti PRIMARY KEY (id),
    CONSTRAINT UQ_dokumenti_korisnik_tip UNIQUE (korisnik_id, tip),
    CONSTRAINT FK_dokumenti_korisnici_korisnik_id
        FOREIGN KEY (korisnik_id) REFERENCES korisnici (id) ON DELETE CASCADE
);

CREATE INDEX IX_tokeni_za_reset_lozinke_korisnik_id
    ON tokeni_za_reset_lozinke (korisnik_id);

CREATE INDEX IX_vozila_korisnik_id
    ON vozila (korisnik_id);

CREATE INDEX IX_goriva_vozilo_id
    ON goriva (vozilo_id);

CREATE INDEX IX_goriva_korisnik_id
    ON goriva (korisnik_id);

CREATE INDEX IX_servisi_vozilo_id
    ON servisi (vozilo_id);

CREATE INDEX IX_servisi_korisnik_id
    ON servisi (korisnik_id);

CREATE INDEX IX_historija_registracija_vozilo_id
    ON historija_registracija (vozilo_id);

CREATE INDEX IX_dokumenti_korisnik_id
    ON dokumenti (korisnik_id);
