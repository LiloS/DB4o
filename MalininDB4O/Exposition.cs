using System;
using System.Collections.Generic;
using Db4objects.Db4o;
using Db4objects.Db4o.Activation;
using Db4objects.Db4o.TA;

namespace MalininDB4O
{
  class Exposition : IActivatable
  {
    [Transient]
    private IActivator activator;

    private readonly Gallery gallery;
    private readonly Painting painting;
    private readonly List<TimeFrame> timeFrames = new List<TimeFrame>();

    public Exposition(Gallery gallery, Painting painting, DateTime startDate, DateTime endDate)
    {
      this.gallery = gallery;
      this.painting = painting;
      timeFrames.Add(new TimeFrame(startDate, endDate));
    }

    public Painting Painting
    {
      get
      {
        Activate(ActivationPurpose.Read);
        return painting;
      }
    }

    public Gallery Gallery
    {
      get
      {
        Activate(ActivationPurpose.Read);
        return gallery;
      }
    }


    public List<TimeFrame> TimeFrames
    {
      get
      {
        Activate(ActivationPurpose.Read);
        return timeFrames;
      }
    }

    public void AddTime(DateTime startDate, DateTime endDate)
    {
      timeFrames.Add(new TimeFrame(startDate, endDate));
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

    public class TimeFrame : IActivatable
    {
      [Transient]
      private IActivator activator;

      private DateTime startDate = DateTime.Now;
      private DateTime endDate = DateTime.Now;

      public TimeFrame(DateTime startDate, DateTime endDate)
      {
        this.startDate = startDate;
        this.endDate = endDate;
      }

      public DateTime StartDate
      {
        get
        {
          Activate(ActivationPurpose.Read);
          return startDate;
        }
      }

      public DateTime EndDate
      {
        get
        {
          Activate(ActivationPurpose.Read);
          return endDate;
        }
      }

      public override string ToString()
      {
        Activate(ActivationPurpose.Read);
        return string.Format("Start: '{0}' End: '{1}'", startDate, endDate);
      }

      public bool ContainDates(TimeFrame otherTimeFrame)
      {
        Activate(ActivationPurpose.Read);
        if (startDate <= otherTimeFrame.startDate && endDate >= otherTimeFrame.endDate)
        {
          return true;
        }
        return false;
      }

      // Omitting any of the following operator overloads 
      // violates rule: OverrideMethodsOnComparableTypes.
      public static bool operator ==(TimeFrame left, TimeFrame right)
      {
        if (object.ReferenceEquals(left, null))
        {
          return object.ReferenceEquals(right, null);
        }
        return left.Equals(right);
      }
      public static bool operator !=(TimeFrame left, TimeFrame right)
      {
        return !(left == right);
      }
      public static bool operator <(TimeFrame left, TimeFrame right)
      {
        return (!left.ContainDates(right));
      }
      public static bool operator >(TimeFrame left, TimeFrame right)
      {
        return (left.ContainDates(right));
      }

      public override bool Equals(object obj)
      {
        Activate(ActivationPurpose.Read);
        var item = obj as TimeFrame;

        if (item == null)
        {
          return false;
        }

        return this.startDate.Equals(item.startDate) && this.endDate.Equals(item.endDate);
      }

      public override int GetHashCode()
      {
        return this.startDate.GetHashCode() + this.endDate.GetHashCode();
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
}
