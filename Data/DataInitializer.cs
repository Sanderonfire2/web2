using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SchoolPortfolio.Models;

namespace SchoolPortfolio.Data;

public static class DataInitializer
{
    public static void Initialiseer(IServiceProvider services)
    {
        using var db = services.GetRequiredService<SiteDbContext>();

        // Verzeker de DB-tabel voor migratiegeschiedenis, zodat Migrate() op een
        // schone server niet faalt met "no such table: __EFMigrationsHistory".
        var dbPad = db.Database.GetDbConnection().DataSource ?? "onbekend";

        try
        {
            db.Database.EnsureCreated();
        }
        catch (Exception ex)
        {
            var logger = services.GetService<Microsoft.Extensions.Logging.ILogger<Program>>();
            logger?.LogError(ex, "EnsureCreated() mislukt voor {DbPad}. Kan schema niet initialiseren.", dbPad);
        }
        try
        {
            db.Database.Migrate();
        }
        catch (Exception ex)
        {
            var logger = services.GetService<Microsoft.Extensions.Logging.ILogger<Program>>();
            logger?.LogError(ex, "Migrate() mislukt voor {DbPad}. Kan schema-history niet aanpassen.", dbPad);
        }
        // Stel alleen standaarddata in op een database die nog geen profiling heeft.
        // Als de tabel niet bestaat, dan heeft de database niet de juiste basisstructuur
        // en mogen we hier geen standaarddata proberen te schrijven.
        if (db.Database.CanConnect())
        {
            try
            {
                if (db.Profielen.Any())
                {
                    // Alleen-rapporten: zet bestaande stageverslag-PDF om naar een Rapport-record,
                    // zodat oudere uploads behouden blijven en in de nieuwe dagrapport-lijst verschijnen.
                    foreach (var stage in db.Stages.Include(s => s.Rapporten).Where(s => s.VerslagPad != null).ToList())
                    {
                        if (stage.Rapporten.Any(r => r.Pad == stage.VerslagPad)) continue;
                        db.Rapporten.Add(new Rapport
                        {
                            StageId = stage.Id,
                            Pad = stage.VerslagPad,
                            Titel = "Stageverslag",
                            Datum = DateTime.Today,
                            IsGepubliceerd = true,
                            Volgorde = 1
                        });
                    }
                    db.SaveChanges();
                    return;
                }
            }
            catch (Exception ex)
            {
                var logger = services.GetService<Microsoft.Extensions.Logging.ILogger<Program>>();
                logger?.LogError(ex, "Injecteer standaarddata mislukt voor {DbPad}.", dbPad);
            }
        }
        else
        {
            var logger = services.GetService<Microsoft.Extensions.Logging.ILogger<Program>>();
            logger?.LogWarning("Geen verbinding met de database {DbPad}; standaarddata wordt overgeslagen.", dbPad);
            return;
        }

        // Alleen bereiken wanneer er nog geen profiel bestaat.
        db.Profielen.Add(new Profiel
            {
                VolledigeNaam = "Sander Van Praet",
                Tagline = "Student elektriciteit & installatietechniek — gewend om secuur en veilig te werken.",
                KorteIntroductie = "Ik studeer elektriciteit en werk rond installatietechniek: installatieschema's, stroomkringen, motorsturingen en meetrapporten volgens de AREI-normen.",
                OverMijTekst = "Elektriciteit trekt me aan omdat het tastbaar werk is: wat ik aanleg en afsluit, moet veilig blijven functioneren, elke dag. Het werk vraagt precisie, een duidelijke aanpak en goedgekeurd materiaal — en dat past bij mij.\n\nIn de lessen bestudeer ik de volledige keten van de installatietechniek: verdeelborden, beveiligingen, aarding, motorsturingen en meettechnieken. Waar het kan, benoem ik verder met een multimeter en teken ik schema's die collega's meteen kunnen hergebruiken.\n\nNaast school werk ik liever op een bouwplaats of in een werkstraat: ik leer het snelst door te zien hoe een ervaren elektricien zijn werk goed plant, zijn materiaal klaarzetten en zijn installatie afwerkt tot het feilloos door de keuring loopt.",
                Woonplaats = "Roeselare, België",
                Email = "sander@vanpraet.be",
                LinkedInUrl = "https://www.linkedin.com/in/sandervanpraet",
                ProfielfotoPad = "/img/profiel.svg",
                BijgewerktOp = DateTime.Now
        });

        db.SaveChanges();

        var project1 = new Project
            {
                Titel = "Installatiestudie nieuwbouwwoning",
            Slug = "installatiestudie-woning",
            Categorie = "Electrische installaties",
            Periode = "2026 · Semester 2",
            Rol = "Ontwerp en berekening, solo",
            Technologieen = "Installatieschema's, belastingberekeningen, kabelberekening",
            KorteSamenvatting = "Complete installatiestudie voor een nieuwbouwwoning: verdeelbord, beveiligingen, aardingspunten en berekening van alle kabelsecties.",
            Omschrijving = "Voor deze opdracht ontwierp ik de volledige elektrische installatie van een nieuwbouwwoning met vier slaapkamers, garage en tuinzicht op het gelijkvloers.\n\nHet werk omvat een schema van het verdeelbord met kringindeling, een berekening van de piekbelasting, het uitkiezen van differentieelbeveiligingen en zekeringen, en de berekening van de kabeldoorsnede per kring op basis van lengte en het belastingsprofiel.\n\nHet eindrapport bevat een meetplan voor de aardingsweerstand en een controlelijst die tijdens de zelfcontrole op het oefenpaneel wordt gebruikt.",
            Uitdaging = "De eerste plannen hielden geen rekening met toekomstige uitbreiding in de garage. Nieuwe berekeningen leidden tot een herziene kringverdeling met reservezones en een vastgelegde kring voor een later aan te sluiten laadpunt.",
            Geleerd = "Hoe belangrijk het is om eerst een volledig belastingsplan op te stellen en pas daarna kabels te kiezen, en dat een schema soms ontbrekende informatie verbergt zonder dat het direct opvalt.",
            CoverPad = "/img/kunst/1.svg",
            IsUitgelicht = true,
            Volgorde = 1,
            BijgewerktOp = DateTime.Now
        };


        var project2 = new Project
        {
            Titel = "Motorsturing met herstartblok",
            Slug = "motorsturing",
            Categorie = "Motorsturing",
            Periode = "2025 · Semester 1",
            Rol = "Bouw en test, werkopdracht (2)",
            Technologieen = "Contactorschema, drukknopen, thermische beveiliging",
            KorteSamenvatting = "Een draaistroommotorsturing met klokcontactblok: rechts-links draaien, thermische beveiliging en herstart na een korte stroomuitval.",
            Omschrijving = "In dit practicum bouwden we met z'n twee een motorsturing op een oefenpaneel: drie drukknopen (start rechts, start links en noodstop) sturen een draaistroommotor aan via twee schakelcontactoren.\n\nDe sturing bevat thermische beveiliging in de drie fasen en een vergrendeling die verhindert dat beide contactoren tegelijk aantrekken — wat anders een kortsluiting geeft.\n\nAan de hand van een extra situatiesheet heeft onze begeleider het gedrag bij een korte stroomuitval doorgenomen; de oplossing was een herstartblok dat de motor alleen opnieuw laat opstarten na een veilige vertraging.",
            Uitdaging = "Het blokkeren van gelijktijdig aantrekken van beide contactoren bleek op papier naar het scherm fout te lopen; pas na tests met een lage proefspanning verdween dit gedrag van het paneel.",
            Geleerd = "Contactorschema's lezen met oog voor storingsoorzaak, en hoe belangrijk een volledige veiligheidsketen is vóórdat het paneel onder spanning gaat.",
            CoverPad = "/img/kunst/2.svg",
            IsUitgelicht = true,
            Volgorde = 2,
            BijgewerktOp = DateTime.Now
        };


        var project3 = new Project
        {
            Titel = "Stopcontact en tuinlicht buiten",
            Slug = "stopcontact-buiten",
            Categorie = "Praktische opdracht",
            Periode = "2025",
            Rol = "Uitvoering, solo",
            Technologieen = "Kabelgoot, IP-geclassificeerd materiaal, aardlekstel",
            KorteSamenvatting = "Praktische opdracht: een weerbestendig stopcontact en een tuinverlichtingskring aanleggen met correcte aarding en beveiliging.",
            Omschrijving = "Voor deze opdracht legde ik een tuinstopcontact en een tuinverlichtingskring aan volgens de moderne normen: plaatsing van kabelgoot langs de muur, aanleg van weerbestendige sokkels, nette aansluiting met aardlekstel en aftest op isolatieweerstand.\n\nDe opdracht begon met een risicoanalyse van de buitenopstelling: vocht, zon en temperatuurschommelingen bepalen de materiaalkeuze en de minimale beschermingsgraad van elk onderdeel. Alles volgt de AREI-richtlijnen voor buiteninstallaties.\n\nAls afsluiter registreerde ik alle meetwaarden: isolatieweerstand, continuïteit van de aardgeleider en controle van de verschiltransformator.",
            Uitdaging = "De eerste kabelroute liep te dicht langs een waterleiding; de oplossing was een verlegging langs de overkant van de muur met behoud van een praktisch bereikbaar verdeelkastje.",
            Geleerd = "Dat buiteninstallaties meer van het materiaal vragen dan van de handvaardigheid, en dat een volledige meetronde op het einde elk vergeten fout aan het licht brengt.",
            CoverPad = "/img/kunst/3.svg",
            IsUitgelicht = true,
            Volgorde = 3,
            BijgewerktOp = DateTime.Now
        };


        var project4 = new Project
        {
            Titel = "Installatieplan leeszone",
            Slug = "installatieplan-leeszone",
            Categorie = "Electrische schema's",
            Periode = "2024 · Semester 2",
            Rol = "Ontwerp en tekening, duo",
            Technologieen = "Eénlijningsschema's, installatiefiches, bouwtekeningen lezen",
            KorteSamenvatting = "Vertrekschema en éénljiningsschema voor een leeszone op de campus, als oefening in het tekenen volgens vaste schema-symbolen.",
            Omschrijving = "Met z'n twee ontwierpen we de elektrische plannen voor een leeszone op de campus: één éénljiningsschema van de voeding en per vertrek een symbolische weergave met alle schakelaars, stopcontacten en armaturen.\n\nWe volgden daarbij de AREI-schema-normen en legden in een legenda vast welk symbool welk onderdeel voorstelt.\n\nAls controle hanterden we een checklist: elk symbool op het plan staat in de legenda, elke kring uit de vertrekkaart komt voor op het schema en de totale belasting blijft binnen de grens die de opdracht vastlegt.",
            Uitdaging = "De plannen bleken los te staan van de werkelijke vertrekafmetingen in de bouwtekening; we hebben alles hernomen en per vertrek vaste margezones vastgelegd.",
            Geleerd = "Dat tekenen volgens vaste regels meer tijd vraagt dan improvissen, en dat een goede legenda veel vragen tijdens de beoordeling vooraf wegneemt.",
            CoverPad = "/img/kunst/4.svg",
            IsUitgelicht = false,
            Volgorde = 4,
            BijgewerktOp = DateTime.Now
        };


        db.AddRange(project1, project2, project3, project4);

        project1.Afbeeldingen.Add(new GallerijAfbeelding { Pad = "/img/kunst/2.svg", Onderschrift = "Eénljiningsschema van het verdeelbord", Volgorde = 1 });
        project1.Afbeeldingen.Add(new GallerijAfbeelding { Pad = "/img/kunst/3.svg", Onderschrift = "Berekeningstabel kabeldoorsnede", Volgorde = 2 });
        project1.Afbeeldingen.Add(new GallerijAfbeelding { Pad = "/img/kunst/5.svg", Onderschrift = "Oefenpaneel met testleidingen", Volgorde = 3 });

        project2.Afbeeldingen.Add(new GallerijAfbeelding { Pad = "/img/kunst/6.svg", Onderschrift = "Oefenpaneel met contactorschema", Volgorde = 1 });
        project2.Afbeeldingen.Add(new GallerijAfbeelding { Pad = "/img/kunst/7.svg", Onderschrift = "Siemensstuurpaneel", Volgorde = 2 });

        project3.Afbeeldingen.Add(new GallerijAfbeelding { Pad = "/img/kunst/8.svg", Onderschrift = "Meetresultaat isolatieweerstand", Volgorde = 1 });
        project3.Afbeeldingen.Add(new GallerijAfbeelding { Pad = "/img/kunst/1.svg", Onderschrift = "Aangelegde tuinstopcontact", Volgorde = 2 });

        project4.Afbeeldingen.Add(new GallerijAfbeelding { Pad = "/img/kunst/4.svg", Onderschrift = "Vertrekschema", Volgorde = 1 });
        project4.Afbeeldingen.Add(new GallerijAfbeelding { Pad = "/img/kunst/6.svg", Onderschrift = "Technische ruimte", Volgorde = 2 });

        db.Opleidingen.AddRange(
            new Opleiding
            {
                Periode = "2023 — heden",
                Titel = "Graduaat Elektriciteit — installatietechniek",
                Instelling = "VIVES Hogeschool",
                Locatie = "Kortrijk",
                Omschrijving = "Kernvakken: elektrische installaties en meettechniek, stroomkringen en elektrische machines, installatiestudies volgens de AREI-norm, industriële elektriciteit en veiligheid op de arbeidsplaats. Elk semester eindigt met een praktijkweek met opdrachten op de oefenpanelen.",
                Volgorde = 1
            },
            new Opleiding
            {
                Periode = "2021 — 2023",
                Titel = "Secundair onderwijs — elektriciteit",
                Instelling = "Centrumschool West-Vlaanderen",
                Locatie = "Roeselare",
                Omschrijving = "Basispraktijk van elektrische installaties: bedradingstechniek, schema's lezen, verbindingen leggen en veilig werken met spanningvoerende delen. Hier groeide het besluit om verder te studeren in de elektriciteitssector.",
                Volgorde = 2
            },
            new Opleiding
            {
                Periode = "2024 · veiligheidsattest",
                Titel = "Attest veiligheid elektrische installaties (basis)",
                Instelling = null,
                Locatie = null,
                Omschrijving = "Dit opleidingsonderdeel heeft alles rond veilig werken nabij elektrische installaties binnengebracht en besloten met het basisattest.",
                Volgorde = 3
            });

        db.Stages.Add(new Stage
        {
            Periode = "2026 · april — juni",
            Bedrijf = "Elektro Vanstapel",
            Functie = "Stagiair elektricien",
            Locatie = "Roeselare",
            Omschrijving = "Vanaf april loopt mijn stageperiode bij een elektrobedrijf dat residentiële en kleine professionele installaties aanlegt en onderhoudt. Ik vergezel het team op bouwplaatsen, werk mee aan de opbouw van verdeelborden en leer het proces van een keuringsrapport kennen.\n\nElke maand staat een ander type bouwplaats centraal — nieuwbouw, renovatie en kleine tertiaire werken — zodat ik de praktijk in een breder licht leer kennen.",
            Taken = "Meewerken aan de opbouw en bedrading van verdeelborden\nInstallaties aanbrengen en meterstanden opnemen\nControle van aarding en beveiliging vóór de oplevering\nMeewerken aan keuringsrapporten en de voorbereiding van keuringen",
            Volgorde = 1
        });

        db.Werkervaringen.AddRange(
            new Werkervaring
            {
                Periode = "2022 — 2023",
                Bedrijf = "Transportbedrijf De Vries",
                Functie = "Vakantiewerker — technische dienst",
                Locatie = "Roeselare",
                Omschrijving = "Assistentie bij het klaarmaken en vervoeren van materiaal voor dak- en buitenwerken, en het opruimen van gereedschap na afloop. Een leerschool in zorgvuldig omgaan met materiaal en in teamafspraken.",
                Volgorde = 1
            },
            new Werkervaring
            {
                Periode = "2021",
                Bedrijf = "Delhaize Roeselare",
                Functie = "Studentenjob — vulwerk",
                Locatie = "Roeselare",
                Omschrijving = "Vullen en opruimen van het droogvoedingsassortiment en hulp bij bevoorrading tijdens de piekweken van de zomer.",
                Volgorde = 2
            });

        db.PaginaInhouden.AddRange(
            new PaginaInhoud { Sleutel = "intro-projecten", Inhoud = "School- en oefenprojecten uit mijn opleiding elektriciteit: installatiestudies, stroomkringen, motorsturingen en praktische werken op de oefenpanelen." },
            new PaginaInhoud { Sleutel = "intro-schoolcarriere", Inhoud = "Van de eerste draad in het secundair onderwijs tot een graduaatprogramma met praktijkweken en een echte stageperiode." },
            new PaginaInhoud { Sleutel = "intro-stage", Inhoud = "In de laatste fase van de opleiding loopt een volledige stageperiode bij een elektrobedrijf. Hier lees je waar die plaatsvindt en wat ik er ga doen." },
            new PaginaInhoud { Sleutel = "intro-werkervaring", Inhoud = "Studentenjobs leerden me meer over werken dan een handboek kan: stiptheid, samenwerken en kalmte onder druk." },
            new PaginaInhoud { Sleutel = "intro-overmij", Inhoud = "Een korte kennismaking: wie ik ben, wat ik studeer en waar ik naartoe werk." },
            new PaginaInhoud { Sleutel = "intro-contact", Inhoud = "Ben je op zoek naar een stagiair of elektricien in opleiding, of heb je een vraag over een project op deze site? Laat gerust een bericht na." });

        db.SaveChanges();
    }
}
