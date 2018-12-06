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
        throw new ArgumentNullException(nameof(name));
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
      return $"Artist: '{name}'";
    }

    public override bool Equals(object obj)
    {
      Activate(ActivationPurpose.Read);

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
        activator?.Activate(purpose);
    }
  }
}
