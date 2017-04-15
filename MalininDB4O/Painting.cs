using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Db4objects.Db4o;
using Db4objects.Db4o.Activation;
using Db4objects.Db4o.TA;

namespace MalininDB4O
{
  class Painting : IActivatable
  {
    [Transient]
    private IActivator activator;

    private string title;
    private Artist artist;

    public Painting(string title)
    {
      if (string.IsNullOrEmpty(title))
      {
        throw new ArgumentNullException("title");
      }

      this.title = title;
    }

    public Painting(Artist artist)
    {
      if (artist == null)
      {
        throw new ArgumentNullException("artist");
      }

      this.artist = artist;
    }

    public Painting(string title, Artist artist)
    {
      if (string.IsNullOrEmpty(title))
      {
        throw new ArgumentNullException("title");
      }

      if (artist == null)
      {
        throw new ArgumentNullException("artist");
      }

      this.title = title;
      this.artist = artist;
    }

    public string Title
    {
      get
      {
        Activate(ActivationPurpose.Read);
        return title;
      }
    }

    public Artist Artist
    {
      get
      {
        Activate(ActivationPurpose.Read);
        return artist;
      }
    }

    public override string ToString()
    {
      Activate(ActivationPurpose.Read);
      return string.Format("Painting: title '{0}', {1}", title, artist);
    }

    public override bool Equals(object obj)
    {
      Activate(ActivationPurpose.Read);
      if (obj == null)
      {
        return false;
      }

      var item = obj as Painting;

      if (item == null)
      {
        return false;
      }

      return this.title.Equals(item.title) && this.artist.Equals(item.artist);
    }

    public override int GetHashCode()
    {
      return this.artist.GetHashCode() + this.title.GetHashCode();
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
