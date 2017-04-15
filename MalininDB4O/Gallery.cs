using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Db4objects.Db4o;
using Db4objects.Db4o.Activation;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.TA;

namespace MalininDB4O
{
  class Gallery : IActivatable
  {
    [Transient]
    private IActivator activator;

    private string name;
    private List<Artist> mainArtists = new List<Artist>();
    private List<Exposition> expositions = new List<Exposition>();

    public Gallery()
    {
    }

    public Gallery(string name)
    {
      if (string.IsNullOrEmpty(name))
      {
        throw new ArgumentNullException("name");
      }

      this.name = name;
    }

    public Gallery(string name, List<Artist> mainArtists)
    {
      if (string.IsNullOrEmpty(name))
      {
        throw new ArgumentNullException("name");
      }
      if (mainArtists == null)
      {
        throw new ArgumentNullException("mainArtist");
      }

      this.name = name;
      this.mainArtists = mainArtists;
    }

    public string Name
    {
      get
      {
        Activate(ActivationPurpose.Read);
        return name;
      }
    }

    public List<Artist> MainArtists
    {
      get
      {
        Activate(ActivationPurpose.Read);
        return mainArtists;
      }
    }

    public void AssignMainArtist(Artist artist)
    {
      this.mainArtists.Add(artist);
    }

    /// <summary>
    /// Adds painting and timeframe to current gallery exposition;
    /// </summary>
    public void AddPainting(Painting painting, DateTime startDate, DateTime endDate)
    {
      foreach (Exposition exposition in expositions)
      {
        if (exposition.Painting.Equals(painting))
        {
          exposition.AddTime(startDate, endDate);
          return;
        }
      }
      
      expositions.Add(new Exposition(this, painting, startDate, endDate));
    }

    public List<Exposition> Expositions
    {
      get
      {
        Activate(ActivationPurpose.Read);
        return expositions;
      }
    }

    public override string ToString()
    {
      Activate(ActivationPurpose.Read);
      return string.Format("Gallery: '{0}'", name);
    }  

    public override bool Equals(object obj)
    {
      Activate(ActivationPurpose.Read);
      var item = obj as Gallery;

      if (item == null)
      {
        return false;
      }

      return this.name.Equals(item.name);
    }

    public override int GetHashCode()
    {
      return this.name.GetHashCode();
    }

    public void Bind(IActivator activator)
    {
      if (this.activator == activator)
      {
        return;
      }
      if (activator != null && null != this.activator)
      {
        throw new System.InvalidOperationException();
      }
      this.activator = activator;
    }

    public void Activate(ActivationPurpose purpose)
    {
      if (activator != null)
      {
        activator.Activate(purpose);
      }
    }

  }
}
