CREATE TABLE `servisi` (
  `id` int NOT NULL AUTO_INCREMENT,
  `tip` varchar(150) NOT NULL,
  `datum` datetime(6) NOT NULL,
  `kilometraza` int NOT NULL,
  `cijena` decimal(10,2) NOT NULL,
  `serviser` varchar(150) NOT NULL,
  `napomena` varchar(2000) NULL,
  `putanja_racuna` varchar(500) NULL,
  `vozilo_id` int NOT NULL,
  `korisnik_id` int NOT NULL,
  CONSTRAINT `PK_servisi` PRIMARY KEY (`id`),
  CONSTRAINT `FK_servisi_vozila_vozilo_id` FOREIGN KEY (`vozilo_id`) REFERENCES `vozila` (`id`) ON DELETE CASCADE,
  CONSTRAINT `FK_servisi_korisnici_korisnik_id` FOREIGN KEY (`korisnik_id`) REFERENCES `korisnici` (`id`) ON DELETE CASCADE
);

CREATE INDEX `IX_servisi_vozilo_id` ON `servisi` (`vozilo_id`);
CREATE INDEX `IX_servisi_korisnik_id` ON `servisi` (`korisnik_id`);
