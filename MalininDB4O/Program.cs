using System;
using System.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Db4objects.Db4o;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Diagnostic;
using Db4objects.Db4o.Linq;
using Db4objects.Db4o.Query;
using Db4objects.Db4o.TA;
using log4net;
using log4net.Config;

namespace MalininDB4O
{
  class Program
  {
    /// <summary>
    /// Logger Updated.
    /// </summary>
    private static readonly ILog log = LogManager.GetLogger("Main");

    static void Main(string[] args)
    {
      XmlConfigurator.Configure();

      var pathToDbFile = ConfigurationManager.AppSettings["PathToDBFile"];
      IEmbeddedConfiguration configuration = Db4oEmbedded.NewConfiguration();

      // TODO: remove if FillDatabase. Also remember to delete file.
      configuration.File.ReadOnly = true;

      // TODO: AddListener if diagnostic info is needed. 
      //configuration.Common.Diagnostic.AddListener(new DiagnosticToConsole());

      configuration.Common.OptimizeNativeQueries = false;

      // Turn on transparent activation.
      configuration.Common.Add(new TransparentActivationSupport());

      // Creating indexes.
      configuration.Common.ObjectClass(typeof(Artist)).ObjectField("name").Indexed(true);
      configuration.Common.ObjectClass(typeof(Gallery)).ObjectField("name").Indexed(true);
      configuration.Common.ObjectClass(typeof(Painting)).ObjectField("title").Indexed(true);
      configuration.Common.ObjectClass(typeof(Painting)).ObjectField("artist").Indexed(true);
      configuration.Common.ObjectClass(typeof(Exposition)).ObjectField("gallery").Indexed(true);
      configuration.Common.ObjectClass(typeof(Exposition)).ObjectField("painting").Indexed(true);
      configuration.Common.ObjectClass(typeof(Exposition)).ObjectField("timeFrames").Indexed(false);
      configuration.Common.ObjectClass(typeof(Gallery)).ObjectField("expositions").Indexed(false);

      //configuration.Common.ObjectClass(typeof(Artist)).CallConstructor(true);
      configuration.Common.Queries.EvaluationMode(QueryEvaluationMode.Lazy);

      IObjectContainer db = Db4oEmbedded.OpenFile(configuration, pathToDbFile);
      try
      {
        //FillDatabase(db);
        ShowAllPaintings(db);
        PaintingsByArtist(db, new Artist("Artist1"));
        DaysOfPaintingInGallery(db, new Painting("Painting1_1", new Artist("Artist1")),
          new Gallery("Gallery1"));
        AmountOfPaintingsInGalleryDuringTimeFrame(db, new Gallery("Gallery3"),
          new DateTime(2000, 04, 10), new DateTime(2000, 04, 12));
        MainArtists(db);
        AllPaintingsByGalleryMainArtistsInGalleryDuringTimeFrame(db,
          new Gallery("Gallery1"), new DateTime(2000, 04, 3), new DateTime(2000, 04, 4));
        AllGalleriesByArtist(db, new Artist("Artist3"));
        AllArtistsWithAtLeast10PaintingsDuringTimePeriodInGallery(db,
          new Gallery("Gallery1"), new DateTime(2000, 04, 10), new DateTime(2000, 04, 12));
        GalleriesWithPaintingsOfArtistDuringTimeframe(db, new Artist("Artist5"),
          new DateTime(2000, 04, 3), new DateTime(2000, 04, 4));
        ArtistsWithPaintingsFromVariousGalleries(db);
      }
      finally
      {
        db.Close();
        log.Info("Press any key to continue...");
        Console.ReadKey();
      }
    }

    public static void FillDatabase(IObjectContainer db)
    {
      log.Info("********************************************************************");
      var stopwatch = new Stopwatch();
      stopwatch.Start();

      // Creates galleries
      var galery1 = new Gallery("Gallery1");
      var galery2 = new Gallery("Gallery2");
      var galery3 = new Gallery("Gallery3");
      var galery4 = new Gallery("Gallery4");
      var galery5 = new Gallery("Gallery5");
      var galery6 = new Gallery("Gallery6");

      var allGalleries = new[] { galery1, galery2, galery3, galery4 , galery5, galery6};
      var rand = new Random();

      // Creates artists.
      for (int i = 0; i < 10; i++)
      {
        var artist = new Artist("Artist"+i);
        // Creates paintings for each artist.

        int amountOfExp = rand.Next(1, 20);

        for (int j = 0; j < amountOfExp; j++)
        {
          var painting = new Painting("Painting"+j+"_"+i, artist);
          var startDateTime = new DateTime(2000, 04, 1);
          var endDateTime = new DateTime(2000, 04, 10);

          int amountOfExp2 = rand.Next(0, 20);

          if (amountOfExp2 == 0)
          {
            db.Store(painting);
          }

          // Create exposition dates of painting.
          for (int k = 0; k < amountOfExp2; k++)
          {
            var randomGallery = allGalleries[rand.Next(0, allGalleries.Length)];
            randomGallery.AddPainting(painting,
                startDateTime.AddDays(new Random().Next(10)),
                endDateTime.AddDays(new Random().Next(10)));
            startDateTime = startDateTime.AddDays(20);
            endDateTime = endDateTime.AddDays(30);
          }
        }   
      }

      // Creates paintings without artist

      for (int j = 0; j < rand.Next(1, 20); j++)
      {
        var painting = new Painting("Painting" + j);
        var startDateTime = new DateTime(2000, 04, 1);
        var endDateTime = new DateTime(2000, 04, 10);

        int amountOfExp = rand.Next(0, 4);

        if (amountOfExp == 0)
        {
          db.Store(painting);
        }
        // Create exposition dates of painting.
        for (int k = 0; k < amountOfExp; k++)
        {
          var randomGallery = allGalleries[rand.Next(0, allGalleries.Length)];
          randomGallery.AddPainting(painting,
              startDateTime.AddDays(new Random().Next(10)),
              endDateTime.AddDays(new Random().Next(10)));
          startDateTime = startDateTime.AddDays(20);
          endDateTime = endDateTime.AddDays(30);
        }
      }   

      galery1.AssignMainArtist(new Artist("Artist1"));
      galery1.AssignMainArtist(new Artist("Artist2"));
      galery2.AssignMainArtist(new Artist("Artist2"));
      galery2.AssignMainArtist(new Artist("Artist3"));
      galery3.AssignMainArtist(new Artist("Artist3"));
      galery4.AssignMainArtist(new Artist("Artist2"));

      db.Store(galery1);
      db.Store(galery2);
      db.Store(galery3);
      db.Store(galery4);
      db.Store(galery5);
      db.Store(galery6);

      stopwatch.Stop();
      log.InfoFormat("Time elapsed: {0}", stopwatch.Elapsed);
    }

    /// <summary>
    /// 1.1. Вывести все картины
    /// By example
    /// </summary>
    public static void ShowAllPaintings(IObjectContainer db)
    {
      log.Info("********************************************************************");
      var stopwatch = new Stopwatch();
      stopwatch.Start();

      //Query by Example (QBE)
      IList<Painting> allPaintings = db.Query<Painting>(typeof(Painting));

      stopwatch.Stop();

      log.Info("All paintings:");

      foreach (var painting in allPaintings)
      {
        log.Info(painting);
      }

      log.InfoFormat("Time elapsed: {0}", stopwatch.Elapsed);
    }

    /// <summary>
    /// 1.2. Вывести все картины художника А
    /// By example
    /// </summary>
    public static void PaintingsByArtist(IObjectContainer db, Artist artist)
    {
      log.Info("********************************************************************");
      var stopwatch = new Stopwatch();
      stopwatch.Start();
      var templatePainting = new Painting(artist);

      //Query by Example (QBE)
      IObjectSet result = db.QueryByExample(templatePainting);

      stopwatch.Stop();

      log.Info("All paintings of "+artist+":");

      foreach (object item in result)
      {
        var painting = item as Painting;
        if (painting == null)
        {
          break;
        }
        log.Info(painting);
      }
      log.InfoFormat("Time elapsed: {0}", stopwatch.Elapsed);
    }

    /// <summary>
    /// 1.3. Вывести срок пребывания картины А в галерее Б.
    /// Native
    /// </summary>
    public static void DaysOfPaintingInGallery(IObjectContainer db, Painting painting, Gallery gallery)
    {
      log.Info("********************************************************************");
      var stopwatch = new Stopwatch();
      stopwatch.Start();

      // Native Queries.
      IList<Exposition> expositions = db.Query<Exposition>(
        exposition =>
          exposition.Gallery.Equals(gallery) && exposition.Painting.Equals(painting) &&
          exposition.Painting.Artist.Equals(painting.Artist));
      stopwatch.Stop();

      int days = 0;
      if (expositions.Count != 0)
      {
        days =
          expositions.FirstOrDefault().TimeFrames.Sum(
            timeFrames => (timeFrames.EndDate - timeFrames.StartDate).Days);
      }
      log.Info("Painting " + painting + " was in gallery " + gallery + " for " +
                          days + " days.");
      log.InfoFormat("Time elapsed: {0}", stopwatch.Elapsed);

      log.Info("*************************TESTING OF NATIVE QUERY OPTIMIZER WORK************************************");
      stopwatch = new Stopwatch();
      stopwatch.Start();
      gallery = new Gallery("Gallery4");
      // Native Queries.
       expositions = db.Query<Exposition>(
        exposition =>
          exposition.Gallery.Equals(gallery) && exposition.Painting.Equals(painting) &&
          exposition.Painting.Artist.Equals(painting.Artist));
      stopwatch.Stop();

      days = 0;
      if (expositions.Count != 0)
      {
        days =
          expositions.FirstOrDefault().TimeFrames.Sum(
            timeFrames => (timeFrames.EndDate - timeFrames.StartDate).Days);
      }
      log.Info("Painting " + painting + " was in gallery " + gallery + " for " +
                          days + " days.");
      log.InfoFormat("Time elapsed: {0}", stopwatch.Elapsed);
    }

    /// <summary>
    /// 1.4. Количество картин в галерее А с 1.04.2017 до 9.04.2017.
    /// SODA
    /// </summary>
    public static void AmountOfPaintingsInGalleryDuringTimeFrame(IObjectContainer db,
      Gallery gallery, DateTime startDate, DateTime endDate)
    {
      log.Info("********************************************************************");
      var stopwatch = new Stopwatch();
      stopwatch.Start();

      //SODA
      IQuery query = db.Query();
      query.Constrain(typeof(Gallery));
      query.Descend("name").Constrain(gallery.Name);
      query.Descend("expositions")
        .Descend("timeFrames").Descend("startDate").Constrain(startDate).Greater().Not();
      query.Descend("expositions")
        .Descend("timeFrames").Descend("endDate").Constrain(endDate).Greater();
      IQuery expositionQuery = query.Descend("expositions");
      IObjectSet result = expositionQuery.Execute();
      stopwatch.Stop();

      int amountOfPaintings = 0;

      List<Exposition> expositions = result.OfType<List<Exposition>>().FirstOrDefault();

      if (expositions == null)
      {
        log.Info("0 paintings was in gallery " + gallery + " since " + startDate + " to " + endDate);
        log.InfoFormat("Time elapsed: {0}", stopwatch.Elapsed);

        return;
      }

      amountOfPaintings += expositions.Count();
      log.Info(amountOfPaintings + " paintings was in gallery " + gallery + " since " + startDate + " to " + endDate);
      log.InfoFormat("Time elapsed: {0}", stopwatch.Elapsed);
    }

    /// <summary>
    /// 1.5. Вывести список самых известных художников для каждой галереи.
    /// SODA
    /// </summary>
    public static void MainArtists(IObjectContainer db)
    {
      log.Info("********************************************************************");
      var stopwatch = new Stopwatch();
      stopwatch.Start();

      //SODA
      IQuery query = db.Query();
      query.Constrain(typeof(Gallery));
      query.Descend("mainArtists");
      IObjectSet result = query.Execute();
      stopwatch.Stop();

      foreach (Gallery gal in result.OfType<Gallery>())
      {
        log.Info("Main artists of gallery "+ gal);
        foreach (var mainArtist in gal.MainArtists)
        {
          log.Info(mainArtist);
        }
      }

      log.InfoFormat("Time elapsed: {0}", stopwatch.Elapsed);
    }

    /// <summary>
    /// 2.1. Вывести все картины самых известных художников галереи А, 
    /// которые находились в галерее А в период с 1.04.2017 до 9.04.2017
    /// Native
    /// </summary>
    public static void AllPaintingsByGalleryMainArtistsInGalleryDuringTimeFrame(IObjectContainer db,
      Gallery gallery, DateTime startDate, DateTime endDate)
    {
      log.Info("********************************************************************");
      var stopwatch = new Stopwatch();
      stopwatch.Start();

      // Native Queries.
      IList<Exposition> expositions = db.Query<Exposition>(
        exposition =>
          exposition.Gallery.Equals(gallery) &&
          exposition.TimeFrames.Any(
            timeframe => timeframe.StartDate < startDate && timeframe.EndDate > endDate) &&
          exposition.Gallery.MainArtists.Contains(exposition.Painting.Artist));

      stopwatch.Stop();
      log.Info("Paintings of main artists of gallery " + gallery + " :");
      foreach (Exposition exposition in expositions)
      {
        log.Info(exposition.Painting);
      }

      log.InfoFormat("Time elapsed: {0}", stopwatch.Elapsed);
    }

    /// <summary>
    /// 2.2. В каких галереях побывали картины художника А и сроки их пребывания 
    /// (если картина в одной галерее была более одного раза, то сумма дней за каждый раз пребывания).
    /// LINQ
    /// </summary>
    public static void AllGalleriesByArtist(IObjectContainer db, Artist artist)
    {
      log.Info("********************************************************************");
      var stopwatch = new Stopwatch();
      stopwatch.Start();

      // LINQ.
      var results = from Exposition exposition in db
        where
          exposition.Painting.Artist != null && exposition.Painting.Artist.Equals(artist)
        group exposition by exposition.Gallery
        into g
        select new
        {
          Gallery = g.Key,
          Sum = g.Sum(exp => exp.TimeFrames.Sum(
            timeFrames => (timeFrames.EndDate - timeFrames.StartDate).Days))
        };
      stopwatch.Stop();


      log.Info(artist +" paintings was in ");
      foreach (var result in results)
      {
        log.Info(result.Gallery + " days :" + result.Sum);
      }

      log.InfoFormat("Time elapsed: {0}", stopwatch.Elapsed);
    }

    /// <summary>
    /// 2.3. Какие художники, с минимальным количеством написанных картин = 8, 
    /// выставлялись в галерее А с 1.04.2017 по 9.04.2017.
    /// LINQ
    /// </summary>
    public static void AllArtistsWithAtLeast10PaintingsDuringTimePeriodInGallery(
      IObjectContainer db, Gallery gallery, DateTime startDate, DateTime endDate)
    {
      log.Info("********************************************************************");
      var stopwatch = new Stopwatch();
      stopwatch.Start();

      // LINQ.
      var results = from Exposition exposition in db
        where exposition.Gallery.Equals(gallery)
              &&
              exposition.TimeFrames.Any(
                timeFrame =>
                  timeFrame.ContainDates(new Exposition.TimeFrame(startDate, endDate)))
        group exposition by exposition.Painting.Artist
        into g
        select (from Painting painting in db
          where painting.Artist != null && painting.Artist.Equals(g.Key)
          group painting by painting.Artist
          into k
          where k.Count() >= 8
          select k.Key);

      stopwatch.Stop();

      log.Info("Artists with minimum amount of paintings = 10 in gallery "+gallery+" from "+startDate+" to "+endDate);
      foreach (IEnumerable<Artist> result in results)
      {
        foreach (Artist artist in result)
        {
          log.Info(artist + ", ");
        }
      }

      log.InfoFormat("Time elapsed: {0}", stopwatch.Elapsed);
    }

    /// <summary>
    /// 2.4. 2 галереи с максимальным количеством картин художника А в период с 1.04.2017 по 9.04.2017.
    /// LINQ
    /// </summary>
    public static void GalleriesWithPaintingsOfArtistDuringTimeframe(
      IObjectContainer db, Artist artist, DateTime startDate, DateTime endDate)
    {
      log.Info("********************************************************************");
      var stopwatch = new Stopwatch();
      stopwatch.Start();

      // LINQ.
      IEnumerable<Gallery> results = (from Exposition exposition in db
        where exposition.TimeFrames.Any(
          timeFrame =>
            timeFrame.ContainDates(new Exposition.TimeFrame(startDate, endDate)))
              &&
              exposition.Painting.Artist != null
              &&
              exposition.Painting.Artist.Equals(artist)
        group exposition by exposition.Gallery
        into g
        orderby g.Count() descending
        select g.Key).Take(2);
      
      stopwatch.Stop();

      log.Info("Galleries with max amount of paintings of artist "+artist+" from "+startDate+" to "+ endDate);

      foreach (var result in results)
      {
        log.Info(result);
      }

      log.InfoFormat("Time elapsed: {0}", stopwatch.Elapsed);
    }

    /// <summary>
    /// 2.5. 5 художников, чьи картины побывали в большем количестве различных галерей.
    /// LINQ
    /// </summary>
    public static void ArtistsWithPaintingsFromVariousGalleries(IObjectContainer db)
    {
      log.Info("********************************************************************");
      var stopwatch = new Stopwatch();
      stopwatch.Start();

      // LINQ.
      var results = (from Exposition exposition in db
        group exposition by new {artist = exposition.Painting.Artist, gallery = exposition.Gallery}
        into g
        group g by g.Key.artist
        into k
        orderby k.Count() descending
        select k).Take(5);

      stopwatch.Stop();

      log.Info("Artists whose paintings were in various galleries: ");

      foreach (var result in results)
      {
        log.Info(result.Key);
      }

      log.InfoFormat("Time elapsed: {0}.", stopwatch.Elapsed);
    }
  }
}
