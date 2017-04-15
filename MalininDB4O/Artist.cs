using System;
using Db4objects.Db4o;
using Db4objects.Db4o.Activation;
using Db4objects.Db4o.TA;

namespace MalininDB4O
{
  class Artist : IActivatable
  {
    [Transient]
    private IActivator activator;
    private string name;

    public Artist()
    {
    }

    public Artist(string name)
    {
      if (string.IsNullOrEmpty(name))
      {
        throw new ArgumentNullException("name");
      }

      this.name = name;
    }

    public string Name
    {
      get
      {
        Activate(ActivationPurpose.Read);
        return name;
      }
    }

    public override string ToString()
    {
      Activate(ActivationPurpose.Read);
      return string.Format("Artist: '{0}'", name);
    }

    public override bool Equals(object obj)
    {
      Activate(ActivationPurpose.Read);
      if (obj == null)
      {
        return false;
      }

      var item = obj as Artist;

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
