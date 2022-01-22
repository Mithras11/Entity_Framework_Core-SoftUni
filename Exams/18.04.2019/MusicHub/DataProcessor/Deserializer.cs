namespace MusicHub.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using MusicHub.Data.Models;
    using MusicHub.Data.Models.Enums;
    using MusicHub.DataProcessor.ImportDtos;
    using Newtonsoft.Json;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data";

        private const string SuccessfullyImportedWriter
            = "Imported {0}";
        private const string SuccessfullyImportedProducerWithPhone
            = "Imported {0} with phone: {1} produces {2} albums";
        private const string SuccessfullyImportedProducerWithNoPhone
            = "Imported {0} with no phone number produces {1} albums";
        private const string SuccessfullyImportedSong
            = "Imported {0} ({1} genre) with duration {2}";
        private const string SuccessfullyImportedPerformer
            = "Imported {0} ({1} songs)";

        public static string ImportWriters(MusicHubDbContext context, string jsonString)
        {
            var writersDtos = JsonConvert.DeserializeObject<List<ImportWriterDto>>(jsonString);
            var writers = new List<Writer>();
            var sb = new StringBuilder();

            foreach (var writerDto in writersDtos)
            {
                //check if valid via attributes
                if (!IsValid(writerDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;

                }

                var currWriter = new Writer()
                {
                    Name = writerDto.Name,
                    Pseudonym = writerDto.Pseudonym

                };


                writers.Add(currWriter);
                sb.AppendLine(String.Format(SuccessfullyImportedWriter, currWriter.Name));

            }

            context.Writers.AddRange(writers);
            context.SaveChanges();

            return sb.ToString().Trim();

        }




        public static string ImportProducersAlbums(MusicHubDbContext context, string jsonString)
        {
            var producersDtos = JsonConvert.DeserializeObject<List<ImportoProducerDto>>(jsonString);
            var producers = new List<Producer>();
            var sb = new StringBuilder();

            foreach (var producerDto in producersDtos)
            {
                //check if valid via attributes
                if (!IsValid(producerDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;

                }

                var currProducer = new Producer()
                {
                    Name = producerDto.Name,
                    Pseudonym = producerDto.Pseudonym,
                    PhoneNumber = producerDto.PhoneNumber

                };


                bool areAlbumsValid = true;

                foreach (var albumDto in producerDto.Albums)
                {
                    if (!IsValid(albumDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        areAlbumsValid = false;
                        break;
                    }

                    DateTime relDate;
                    bool isValidRelDate = DateTime.TryParseExact(albumDto.ReleaseDate, "dd/MM/yyyy",
                                        CultureInfo.InvariantCulture, DateTimeStyles.None, out relDate);

                    if (!isValidRelDate)
                    {
                        sb.AppendLine(ErrorMessage);
                        //areAlbumsValid = false;
                        //break;

                        continue;
                    }

                    var album = new Album()
                    {
                        Name = albumDto.Name,
                        ReleaseDate = relDate,
                        Producer = currProducer,
                        ProducerId = currProducer.Id

                    };

                    currProducer.Albums.Add(album);

                }

                if (!areAlbumsValid)
                {
                    continue;
                }                


                producers.Add(currProducer);

                if (currProducer.PhoneNumber != null)
                {
                    sb.AppendLine(String.Format(SuccessfullyImportedProducerWithPhone,
                        currProducer.Name, currProducer.PhoneNumber, currProducer.Albums.Count));
                }
                else
                {
                    sb.AppendLine(String.Format(SuccessfullyImportedProducerWithNoPhone,
                        currProducer.Name, currProducer.Albums.Count));
                }

            }

            context.Producers.AddRange(producers);
            context.SaveChanges();

            return sb.ToString().Trim();
        }

        public static string ImportSongs(MusicHubDbContext context, string xmlString)
        {
            var sb = new StringBuilder();

            var serializer = new XmlSerializer(typeof(List<ImportSongDto>), new XmlRootAttribute("Songs"));

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);
            var reader = new StringReader(xmlString);

            using (reader)
            {
                var songsDtos = (List<ImportSongDto>)serializer.Deserialize(reader);

                var songsToAdd = new List<Song>();

                foreach (var songDto in songsDtos)
                {
                    if (!IsValid(songDto))
                    {
                        sb.AppendLine(ErrorMessage);

                        continue;
                    }

                    //validate duration and cast

                    TimeSpan duration;
                    bool isDurationValid = TimeSpan.TryParseExact(
                                          songDto.Duration, "c", CultureInfo.InvariantCulture, out duration);

                    if (!isDurationValid)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    //check creationDate

                    DateTime creDate;
                    bool isValidCreDate = DateTime.TryParseExact(songDto.CreatedOn, "dd/MM/yyyy",
                                        CultureInfo.InvariantCulture, DateTimeStyles.None, out creDate);

                    if (!isValidCreDate)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    //check genre
                    object genre;

                    bool isValidGenre = Enum.TryParse(typeof(Genre), songDto.Genre, out genre);

                    if (!isValidGenre)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }


                    var album = context.Albums.FirstOrDefault(a => a.Id == songDto.AlbumId);
                    var writer = context.Writers.FirstOrDefault(w => w.Id == songDto.WriterId);

                    if (album == null || writer == null)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    var song = new Song()
                    {
                        Name = songDto.Name,
                        Duration = duration,
                        CreatedOn = creDate,
                        Genre = (Genre)genre,
                        AlbumId = songDto.AlbumId,
                        Album = album,
                        WriterId = songDto.WriterId,
                        Writer = writer,
                        Price = songDto.Price
                    };


                    songsToAdd.Add(song);

                    sb.AppendLine(string.Format(SuccessfullyImportedSong, song.Name,
                        song.Genre, song.Duration));
                }

                context.Songs.AddRange(songsToAdd);
                context.SaveChanges();
                return sb.ToString().Trim();


            }
        }

        public static string ImportSongPerformers(MusicHubDbContext context, string xmlString)
        {
            var sb = new StringBuilder();
            var serializer = new XmlSerializer(typeof(List<ImportPerformerDto>), new XmlRootAttribute("Performers"));
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);
            var reader = new StringReader(xmlString);

            using (reader)
            {
                var performersDtos = (List<ImportPerformerDto>)serializer.Deserialize(reader);
                var performersToAdd = new List<Performer>();

                foreach (var performerDto in performersDtos)
                {
                    if (!IsValid(performerDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    var performer = new Performer()
                    {
                        FirstName = performerDto.FirstName,
                        LastName = performerDto.LastName,
                        Age = performerDto.Age,
                        NetWorth = performerDto.NetWorth
                    };

                    var songsPerformers = new List<SongPerformer>();
                    bool areSongsValid = true;

                    foreach (var songDto in performerDto.PerformersSongs)
                    {
                        if (!IsValid(songDto))
                        {
                            sb.AppendLine(ErrorMessage);
                            areSongsValid = false;
                            break;
                        }

                        var song = context.Songs.FirstOrDefault(s => s.Id == songDto.Id);

                        if (song == null)
                        {
                            sb.AppendLine(ErrorMessage);
                            areSongsValid = false;
                            break;
                        }

                        var songPerfomer = new SongPerformer()
                        {
                            Song = song,
                            Perfomer = performer
                        };

                        performer.PerformerSongs.Add(songPerfomer);

                    }

                    if (!areSongsValid)
                    {
                        continue;
                    }


                    performersToAdd.Add(performer);

                    sb.AppendLine(string.Format(SuccessfullyImportedPerformer,
                        performer.FirstName, performer.PerformerSongs.Count));

                }

                context.Performers.AddRange(performersToAdd);
                context.SaveChanges();
                return sb.ToString().Trim();
            }
        }

        private static bool IsValid(object obj)
        {
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(obj);
            var validationResult = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(obj, validationContext, validationResult, true);
            return isValid;
        }
    }
}


