/**
 * Autogenerated by Thrift Compiler (0.14.1)
 *
 * DO NOT EDIT UNLESS YOU ARE SURE THAT YOU KNOW WHAT YOU ARE DOING
 *  @generated
 */

using System.Text;
using System.Threading;
using Thrift.Protocol;
using Thrift.Protocol.Entities;
using Thrift.Protocol.Utilities;

#pragma warning disable IDE0079  // remove unnecessary pragmas
#pragma warning disable IDE1006  // parts of the code use IDL spelling

namespace Jaeger.Thrift.Crossdock
{

  /// <summary>
  /// Each server must include the information about the span it observed.
  /// It can only be omitted from the response if notImplementedError field is not empty.
  /// If the server was instructed to make a downstream call, it must embed the
  /// downstream response in its own response.
  /// </summary>
  public partial class TraceResponse : TBase
  {
    private global::Jaeger.Thrift.Crossdock.ObservedSpan _span;
    private global::Jaeger.Thrift.Crossdock.TraceResponse _downstream;

    public global::Jaeger.Thrift.Crossdock.ObservedSpan Span
    {
      get
      {
        return _span;
      }
      set
      {
        __isset.span = true;
        this._span = value;
      }
    }

    public global::Jaeger.Thrift.Crossdock.TraceResponse Downstream
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

    public string NotImplementedError { get; set; }


    public Isset __isset;
    public struct Isset
    {
      public bool span;
      public bool downstream;
    }

    public TraceResponse()
    {
    }

    public TraceResponse(string notImplementedError) : this()
    {
      this.NotImplementedError = notImplementedError;
    }

    public TraceResponse DeepCopy()
    {
      var tmp8 = new TraceResponse();
      if((Span != null) && __isset.span)
      {
        tmp8.Span = (global::Jaeger.Thrift.Crossdock.ObservedSpan)this.Span.DeepCopy();
      }
      tmp8.__isset.span = this.__isset.span;
      if((Downstream != null) && __isset.downstream)
      {
        tmp8.Downstream = (global::Jaeger.Thrift.Crossdock.TraceResponse)this.Downstream.DeepCopy();
      }
      tmp8.__isset.downstream = this.__isset.downstream;
      if((NotImplementedError != null))
      {
        tmp8.NotImplementedError = this.NotImplementedError;
      }
      return tmp8;
    }

    public async global::System.Threading.Tasks.Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
    {
      iprot.IncrementRecursionDepth();
      try
      {
        bool isset_notImplementedError = false;
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
                Span = new global::Jaeger.Thrift.Crossdock.ObservedSpan();
                await Span.ReadAsync(iprot, cancellationToken);
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 2:
              if (field.Type == TType.Struct)
              {
                Downstream = new global::Jaeger.Thrift.Crossdock.TraceResponse();
                await Downstream.ReadAsync(iprot, cancellationToken);
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 3:
              if (field.Type == TType.String)
              {
                NotImplementedError = await iprot.ReadStringAsync(cancellationToken);
                isset_notImplementedError = true;
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
        if (!isset_notImplementedError)
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
        var struc = new TStruct("TraceResponse");
        await oprot.WriteStructBeginAsync(struc, cancellationToken);
        var field = new TField();
        if((Span != null) && __isset.span)
        {
          field.Name = "span";
          field.Type = TType.Struct;
          field.ID = 1;
          await oprot.WriteFieldBeginAsync(field, cancellationToken);
          await Span.WriteAsync(oprot, cancellationToken);
          await oprot.WriteFieldEndAsync(cancellationToken);
        }
        if((Downstream != null) && __isset.downstream)
        {
          field.Name = "downstream";
          field.Type = TType.Struct;
          field.ID = 2;
          await oprot.WriteFieldBeginAsync(field, cancellationToken);
          await Downstream.WriteAsync(oprot, cancellationToken);
          await oprot.WriteFieldEndAsync(cancellationToken);
        }
        if((NotImplementedError != null))
        {
          field.Name = "notImplementedError";
          field.Type = TType.String;
          field.ID = 3;
          await oprot.WriteFieldBeginAsync(field, cancellationToken);
          await oprot.WriteStringAsync(NotImplementedError, cancellationToken);
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
      if (!(that is TraceResponse other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return ((__isset.span == other.__isset.span) && ((!__isset.span) || (System.Object.Equals(Span, other.Span))))
        && ((__isset.downstream == other.__isset.downstream) && ((!__isset.downstream) || (System.Object.Equals(Downstream, other.Downstream))))
        && System.Object.Equals(NotImplementedError, other.NotImplementedError);
    }

    public override int GetHashCode() {
      int hashcode = 157;
      unchecked {
        if((Span != null) && __isset.span)
        {
          hashcode = (hashcode * 397) + Span.GetHashCode();
        }
        if((Downstream != null) && __isset.downstream)
        {
          hashcode = (hashcode * 397) + Downstream.GetHashCode();
        }
        if((NotImplementedError != null))
        {
          hashcode = (hashcode * 397) + NotImplementedError.GetHashCode();
        }
      }
      return hashcode;
    }

    public override string ToString()
    {
      var sb = new StringBuilder("TraceResponse(");
      int tmp9 = 0;
      if((Span != null) && __isset.span)
      {
        if(0 < tmp9++) { sb.Append(", "); }
        sb.Append("Span: ");
        Span.ToString(sb);
      }
      if((Downstream != null) && __isset.downstream)
      {
        if(0 < tmp9++) { sb.Append(", "); }
        sb.Append("Downstream: ");
        Downstream.ToString(sb);
      }
      if((NotImplementedError != null))
      {
        if(0 < tmp9) { sb.Append(", "); }
        sb.Append("NotImplementedError: ");
        NotImplementedError.ToString(sb);
      }
      sb.Append(')');
      return sb.ToString();
    }
  }

}
