/**
 * Autogenerated by Thrift Compiler (0.12.0)
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

using Thrift.Protocols;
using Thrift.Protocols.Entities;
using Thrift.Protocols.Utilities;
using Thrift.Transports;
using Thrift.Transports.Client;
using Thrift.Transports.Server;


namespace Jaeger.Thrift.Crossdock
{

  public partial class Downstream : TBase
  {
    private Downstream _downstream;

    public string ServiceName { get; set; }

    public string ServerRole { get; set; }

    public string Host { get; set; }

    public string Port { get; set; }

    /// <summary>
    /// 
    /// <seealso cref="Transport"/>
    /// </summary>
    public Transport Transport { get; set; }

    public Downstream Downstream_
    {
      get
      {
        return _downstream;
      }
      set
      {
        __isset.downstream = true;
        this._downstream = value;
      }
    }


    public Isset __isset;
    public struct Isset
    {
      public bool downstream;
    }

    public Downstream()
    {
    }

    public Downstream(string serviceName, string serverRole, string host, string port, Transport transport) : this()
    {
      this.ServiceName = serviceName;
      this.ServerRole = serverRole;
      this.Host = host;
      this.Port = port;
      this.Transport = transport;
    }

    public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
    {
      iprot.IncrementRecursionDepth();
      try
      {
        bool isset_serviceName = false;
        bool isset_serverRole = false;
        bool isset_host = false;
        bool isset_port = false;
        bool isset_transport = false;
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
              if (field.Type == TType.String)
              {
                ServiceName = await iprot.ReadStringAsync(cancellationToken);
                isset_serviceName = true;
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 2:
              if (field.Type == TType.String)
              {
                ServerRole = await iprot.ReadStringAsync(cancellationToken);
                isset_serverRole = true;
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 3:
              if (field.Type == TType.String)
              {
                Host = await iprot.ReadStringAsync(cancellationToken);
                isset_host = true;
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 4:
              if (field.Type == TType.String)
              {
                Port = await iprot.ReadStringAsync(cancellationToken);
                isset_port = true;
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 5:
              if (field.Type == TType.I32)
              {
                Transport = (Transport)await iprot.ReadI32Async(cancellationToken);
                isset_transport = true;
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 6:
              if (field.Type == TType.Struct)
              {
                Downstream_ = new Downstream();
                await Downstream_.ReadAsync(iprot, cancellationToken);
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
        if (!isset_serviceName)
        {
          throw new TProtocolException(TProtocolException.INVALID_DATA);
        }
        if (!isset_serverRole)
        {
          throw new TProtocolException(TProtocolException.INVALID_DATA);
        }
        if (!isset_host)
        {
          throw new TProtocolException(TProtocolException.INVALID_DATA);
        }
        if (!isset_port)
        {
          throw new TProtocolException(TProtocolException.INVALID_DATA);
        }
        if (!isset_transport)
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
        var struc = new TStruct("Downstream");
        await oprot.WriteStructBeginAsync(struc, cancellationToken);
        var field = new TField();
        field.Name = "serviceName";
        field.Type = TType.String;
        field.ID = 1;
        await oprot.WriteFieldBeginAsync(field, cancellationToken);
        await oprot.WriteStringAsync(ServiceName, cancellationToken);
        await oprot.WriteFieldEndAsync(cancellationToken);
        field.Name = "serverRole";
        field.Type = TType.String;
        field.ID = 2;
        await oprot.WriteFieldBeginAsync(field, cancellationToken);
        await oprot.WriteStringAsync(ServerRole, cancellationToken);
        await oprot.WriteFieldEndAsync(cancellationToken);
        field.Name = "host";
        field.Type = TType.String;
        field.ID = 3;
        await oprot.WriteFieldBeginAsync(field, cancellationToken);
        await oprot.WriteStringAsync(Host, cancellationToken);
        await oprot.WriteFieldEndAsync(cancellationToken);
        field.Name = "port";
        field.Type = TType.String;
        field.ID = 4;
        await oprot.WriteFieldBeginAsync(field, cancellationToken);
        await oprot.WriteStringAsync(Port, cancellationToken);
        await oprot.WriteFieldEndAsync(cancellationToken);
        field.Name = "transport";
        field.Type = TType.I32;
        field.ID = 5;
        await oprot.WriteFieldBeginAsync(field, cancellationToken);
        await oprot.WriteI32Async((int)Transport, cancellationToken);
        await oprot.WriteFieldEndAsync(cancellationToken);
        if (Downstream_ != null && __isset.downstream)
        {
          field.Name = "downstream";
          field.Type = TType.Struct;
          field.ID = 6;
          await oprot.WriteFieldBeginAsync(field, cancellationToken);
          await Downstream_.WriteAsync(oprot, cancellationToken);
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

    public override string ToString()
    {
      var sb = new StringBuilder("Downstream(");
      sb.Append(", ServiceName: ");
      sb.Append(ServiceName);
      sb.Append(", ServerRole: ");
      sb.Append(ServerRole);
      sb.Append(", Host: ");
      sb.Append(Host);
      sb.Append(", Port: ");
      sb.Append(Port);
      sb.Append(", Transport: ");
      sb.Append(Transport);
      if (Downstream_ != null && __isset.downstream)
      {
        sb.Append(", Downstream_: ");
        sb.Append(Downstream_);
      }
      sb.Append(")");
      return sb.ToString();
    }
  }

}
