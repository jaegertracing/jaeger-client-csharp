/**
 * Autogenerated by Thrift Compiler (0.13.0)
 *
 * DO NOT EDIT UNLESS YOU ARE SURE THAT YOU KNOW WHAT YOU ARE DOING
 *  @generated
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Thrift;
using Thrift.Collections;

using Thrift.Protocol;
using Thrift.Protocol.Entities;
using Thrift.Protocol.Utilities;
using Thrift.Transport;
using Thrift.Transport.Client;
using Thrift.Transport.Server;
using Thrift.Processor;


namespace Jaeger.Thrift.Agent
{

  public partial class ThrottlingResponse : TBase
  {

    public ThrottlingConfig DefaultConfig { get; set; }

    public List<ServiceThrottlingConfig> ServiceConfigs { get; set; }

    public ThrottlingResponse()
    {
    }

    public ThrottlingResponse(ThrottlingConfig defaultConfig, List<ServiceThrottlingConfig> serviceConfigs) : this()
    {
      this.DefaultConfig = defaultConfig;
      this.ServiceConfigs = serviceConfigs;
    }

    public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
    {
      iprot.IncrementRecursionDepth();
      try
      {
        bool isset_defaultConfig = false;
        bool isset_serviceConfigs = false;
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
              if (field.Type == TType.Struct)
              {
                DefaultConfig = new ThrottlingConfig();
                await DefaultConfig.ReadAsync(iprot, cancellationToken);
                isset_defaultConfig = true;
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 2:
              if (field.Type == TType.List)
              {
                {
                  TList _list0 = await iprot.ReadListBeginAsync(cancellationToken);
                  ServiceConfigs = new List<ServiceThrottlingConfig>(_list0.Count);
                  for(int _i1 = 0; _i1 < _list0.Count; ++_i1)
                  {
                    ServiceThrottlingConfig _elem2;
                    _elem2 = new ServiceThrottlingConfig();
                    await _elem2.ReadAsync(iprot, cancellationToken);
                    ServiceConfigs.Add(_elem2);
                  }
                  await iprot.ReadListEndAsync(cancellationToken);
                }
                isset_serviceConfigs = true;
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
        if (!isset_defaultConfig)
        {
          throw new TProtocolException(TProtocolException.INVALID_DATA);
        }
        if (!isset_serviceConfigs)
        {
          throw new TProtocolException(TProtocolException.INVALID_DATA);
        }
      }
      finally
      {
        iprot.DecrementRecursionDepth();
      }
    }

    public async Task WriteAsync(TProtocol oprot, CancellationToken cancellationToken)
    {
      oprot.IncrementRecursionDepth();
      try
      {
        var struc = new TStruct("ThrottlingResponse");
        await oprot.WriteStructBeginAsync(struc, cancellationToken);
        var field = new TField();
        field.Name = "defaultConfig";
        field.Type = TType.Struct;
        field.ID = 1;
        await oprot.WriteFieldBeginAsync(field, cancellationToken);
        await DefaultConfig.WriteAsync(oprot, cancellationToken);
        await oprot.WriteFieldEndAsync(cancellationToken);
        field.Name = "serviceConfigs";
        field.Type = TType.List;
        field.ID = 2;
        await oprot.WriteFieldBeginAsync(field, cancellationToken);
        {
          await oprot.WriteListBeginAsync(new TList(TType.Struct, ServiceConfigs.Count), cancellationToken);
          foreach (ServiceThrottlingConfig _iter3 in ServiceConfigs)
          {
            await _iter3.WriteAsync(oprot, cancellationToken);
          }
          await oprot.WriteListEndAsync(cancellationToken);
        }
        await oprot.WriteFieldEndAsync(cancellationToken);
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
      var other = that as ThrottlingResponse;
      if (other == null) return false;
      if (ReferenceEquals(this, other)) return true;
      return System.Object.Equals(DefaultConfig, other.DefaultConfig)
        && TCollections.Equals(ServiceConfigs, other.ServiceConfigs);
    }

    public override int GetHashCode() {
      int hashcode = 157;
      unchecked {
        hashcode = (hashcode * 397) + DefaultConfig.GetHashCode();
        hashcode = (hashcode * 397) + TCollections.GetHashCode(ServiceConfigs);
      }
      return hashcode;
    }

    public override string ToString()
    {
      var sb = new StringBuilder("ThrottlingResponse(");
      sb.Append(", DefaultConfig: ");
      sb.Append(DefaultConfig== null ? "<null>" : DefaultConfig.ToString());
      sb.Append(", ServiceConfigs: ");
      sb.Append(ServiceConfigs);
      sb.Append(")");
      return sb.ToString();
    }
  }

}
