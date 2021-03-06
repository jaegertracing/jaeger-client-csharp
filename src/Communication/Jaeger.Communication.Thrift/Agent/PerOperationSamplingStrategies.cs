/**
 * Autogenerated by Thrift Compiler (0.14.1)
 *
 * DO NOT EDIT UNLESS YOU ARE SURE THAT YOU KNOW WHAT YOU ARE DOING
 *  @generated
 */

using System.Collections.Generic;
using System.Text;
using System.Threading;
using Thrift.Collections;

using Thrift.Protocol;
using Thrift.Protocol.Entities;
using Thrift.Protocol.Utilities;

#pragma warning disable IDE0079  // remove unnecessary pragmas
#pragma warning disable IDE1006  // parts of the code use IDL spelling

namespace Jaeger.Thrift.Agent
{

  public partial class PerOperationSamplingStrategies : TBase
  {
    private double _defaultUpperBoundTracesPerSecond;

    public double DefaultSamplingProbability { get; set; }

    public double DefaultLowerBoundTracesPerSecond { get; set; }

    public List<global::Jaeger.Thrift.Agent.OperationSamplingStrategy> PerOperationStrategies { get; set; }

    public double DefaultUpperBoundTracesPerSecond
    {
      get
      {
        return _defaultUpperBoundTracesPerSecond;
      }
      set
      {
        __isset.defaultUpperBoundTracesPerSecond = true;
        this._defaultUpperBoundTracesPerSecond = value;
      }
    }


    public Isset __isset;
    public struct Isset
    {
      public bool defaultUpperBoundTracesPerSecond;
    }

    public PerOperationSamplingStrategies()
    {
    }

    public PerOperationSamplingStrategies(double defaultSamplingProbability, double defaultLowerBoundTracesPerSecond, List<global::Jaeger.Thrift.Agent.OperationSamplingStrategy> perOperationStrategies) : this()
    {
      this.DefaultSamplingProbability = defaultSamplingProbability;
      this.DefaultLowerBoundTracesPerSecond = defaultLowerBoundTracesPerSecond;
      this.PerOperationStrategies = perOperationStrategies;
    }

    public PerOperationSamplingStrategies DeepCopy()
    {
      var tmp6 = new PerOperationSamplingStrategies();
      tmp6.DefaultSamplingProbability = this.DefaultSamplingProbability;
      tmp6.DefaultLowerBoundTracesPerSecond = this.DefaultLowerBoundTracesPerSecond;
      if((PerOperationStrategies != null))
      {
        tmp6.PerOperationStrategies = this.PerOperationStrategies.DeepCopy();
      }
      if(__isset.defaultUpperBoundTracesPerSecond)
      {
        tmp6.DefaultUpperBoundTracesPerSecond = this.DefaultUpperBoundTracesPerSecond;
      }
      tmp6.__isset.defaultUpperBoundTracesPerSecond = this.__isset.defaultUpperBoundTracesPerSecond;
      return tmp6;
    }

    public async global::System.Threading.Tasks.Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
    {
      iprot.IncrementRecursionDepth();
      try
      {
        bool isset_defaultSamplingProbability = false;
        bool isset_defaultLowerBoundTracesPerSecond = false;
        bool isset_perOperationStrategies = false;
        TField field;
        await iprot.ReadStructBeginAsync(cancellationToken);
        while (true)
        {
          field = await iprot.ReadFieldBeginAsync(cancellationToken);
          if (field.Type == TType.Stop)
          {
            break;
          }

          switch (field.ID)
          {
            case 1:
              if (field.Type == TType.Double)
              {
                DefaultSamplingProbability = await iprot.ReadDoubleAsync(cancellationToken);
                isset_defaultSamplingProbability = true;
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 2:
              if (field.Type == TType.Double)
              {
                DefaultLowerBoundTracesPerSecond = await iprot.ReadDoubleAsync(cancellationToken);
                isset_defaultLowerBoundTracesPerSecond = true;
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 3:
              if (field.Type == TType.List)
              {
                {
                  TList _list7 = await iprot.ReadListBeginAsync(cancellationToken);
                  PerOperationStrategies = new List<global::Jaeger.Thrift.Agent.OperationSamplingStrategy>(_list7.Count);
                  for(int _i8 = 0; _i8 < _list7.Count; ++_i8)
                  {
                    global::Jaeger.Thrift.Agent.OperationSamplingStrategy _elem9;
                    _elem9 = new global::Jaeger.Thrift.Agent.OperationSamplingStrategy();
                    await _elem9.ReadAsync(iprot, cancellationToken);
                    PerOperationStrategies.Add(_elem9);
                  }
                  await iprot.ReadListEndAsync(cancellationToken);
                }
                isset_perOperationStrategies = true;
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 4:
              if (field.Type == TType.Double)
              {
                DefaultUpperBoundTracesPerSecond = await iprot.ReadDoubleAsync(cancellationToken);
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            default: 
              await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              break;
          }

          await iprot.ReadFieldEndAsync(cancellationToken);
        }

        await iprot.ReadStructEndAsync(cancellationToken);
        if (!isset_defaultSamplingProbability)
        {
          throw new TProtocolException(TProtocolException.INVALID_DATA);
        }
        if (!isset_defaultLowerBoundTracesPerSecond)
        {
          throw new TProtocolException(TProtocolException.INVALID_DATA);
        }
        if (!isset_perOperationStrategies)
        {
          throw new TProtocolException(TProtocolException.INVALID_DATA);
        }
      }
      finally
      {
        iprot.DecrementRecursionDepth();
      }
    }

    public async global::System.Threading.Tasks.Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
    {
      oprot.IncrementRecursionDepth();
      try
      {
        var struc = new TStruct("PerOperationSamplingStrategies");
        await oprot.WriteStructBeginAsync(struc, cancellationToken);
        var field = new TField();
        field.Name = "defaultSamplingProbability";
        field.Type = TType.Double;
        field.ID = 1;
        await oprot.WriteFieldBeginAsync(field, cancellationToken);
        await oprot.WriteDoubleAsync(DefaultSamplingProbability, cancellationToken);
        await oprot.WriteFieldEndAsync(cancellationToken);
        field.Name = "defaultLowerBoundTracesPerSecond";
        field.Type = TType.Double;
        field.ID = 2;
        await oprot.WriteFieldBeginAsync(field, cancellationToken);
        await oprot.WriteDoubleAsync(DefaultLowerBoundTracesPerSecond, cancellationToken);
        await oprot.WriteFieldEndAsync(cancellationToken);
        if((PerOperationStrategies != null))
        {
          field.Name = "perOperationStrategies";
          field.Type = TType.List;
          field.ID = 3;
          await oprot.WriteFieldBeginAsync(field, cancellationToken);
          {
            await oprot.WriteListBeginAsync(new TList(TType.Struct, PerOperationStrategies.Count), cancellationToken);
            foreach (global::Jaeger.Thrift.Agent.OperationSamplingStrategy _iter10 in PerOperationStrategies)
            {
              await _iter10.WriteAsync(oprot, cancellationToken);
            }
            await oprot.WriteListEndAsync(cancellationToken);
          }
          await oprot.WriteFieldEndAsync(cancellationToken);
        }
        if(__isset.defaultUpperBoundTracesPerSecond)
        {
          field.Name = "defaultUpperBoundTracesPerSecond";
          field.Type = TType.Double;
          field.ID = 4;
          await oprot.WriteFieldBeginAsync(field, cancellationToken);
          await oprot.WriteDoubleAsync(DefaultUpperBoundTracesPerSecond, cancellationToken);
          await oprot.WriteFieldEndAsync(cancellationToken);
        }
        await oprot.WriteFieldStopAsync(cancellationToken);
        await oprot.WriteStructEndAsync(cancellationToken);
      }
      finally
      {
        oprot.DecrementRecursionDepth();
      }
    }

    public override bool Equals(object that)
    {
      if (!(that is PerOperationSamplingStrategies other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return System.Object.Equals(DefaultSamplingProbability, other.DefaultSamplingProbability)
        && System.Object.Equals(DefaultLowerBoundTracesPerSecond, other.DefaultLowerBoundTracesPerSecond)
        && TCollections.Equals(PerOperationStrategies, other.PerOperationStrategies)
        && ((__isset.defaultUpperBoundTracesPerSecond == other.__isset.defaultUpperBoundTracesPerSecond) && ((!__isset.defaultUpperBoundTracesPerSecond) || (System.Object.Equals(DefaultUpperBoundTracesPerSecond, other.DefaultUpperBoundTracesPerSecond))));
    }

    public override int GetHashCode() {
      int hashcode = 157;
      unchecked {
        hashcode = (hashcode * 397) + DefaultSamplingProbability.GetHashCode();
        hashcode = (hashcode * 397) + DefaultLowerBoundTracesPerSecond.GetHashCode();
        if((PerOperationStrategies != null))
        {
          hashcode = (hashcode * 397) + TCollections.GetHashCode(PerOperationStrategies);
        }
        if(__isset.defaultUpperBoundTracesPerSecond)
        {
          hashcode = (hashcode * 397) + DefaultUpperBoundTracesPerSecond.GetHashCode();
        }
      }
      return hashcode;
    }

    public override string ToString()
    {
      var sb = new StringBuilder("PerOperationSamplingStrategies(");
      sb.Append(", DefaultSamplingProbability: ");
      DefaultSamplingProbability.ToString(sb);
      sb.Append(", DefaultLowerBoundTracesPerSecond: ");
      DefaultLowerBoundTracesPerSecond.ToString(sb);
      if((PerOperationStrategies != null))
      {
        sb.Append(", PerOperationStrategies: ");
        PerOperationStrategies.ToString(sb);
      }
      if(__isset.defaultUpperBoundTracesPerSecond)
      {
        sb.Append(", DefaultUpperBoundTracesPerSecond: ");
        DefaultUpperBoundTracesPerSecond.ToString(sb);
      }
      sb.Append(')');
      return sb.ToString();
    }
  }

}
