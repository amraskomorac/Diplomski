CREATE TABLE `historija_registracija` (
  `id` int NOT NULL AUTO_INCREMENT,
  `vozilo_id` int NOT NULL,
  `datum_registracije` datetime(6) NOT NULL,
  `datum_isteka_registracije` datetime(6) NOT NULL,
  `cijena` decimal(10,2) NULL,
  `polica_osiguranja` varchar(150) NULL,
  CONSTRAINT `PK_historija_registracija` PRIMARY KEY (`id`),
  CONSTRAINT `FK_historija_registracija_vozila_vozilo_id` FOREIGN KEY (`vozilo_id`) REFERENCES `vozila` (`id`) ON DELETE CASCADE
);

CREATE INDEX `IX_historija_registracija_vozilo_id` ON `historija_registracija` (`vozilo_id`);
