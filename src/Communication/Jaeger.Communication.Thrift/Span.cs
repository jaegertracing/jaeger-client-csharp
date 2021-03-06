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

namespace Jaeger.Thrift
{

  public partial class Span : TBase
  {
    private List<global::Jaeger.Thrift.SpanRef> _references;
    private List<global::Jaeger.Thrift.Tag> _tags;
    private List<global::Jaeger.Thrift.Log> _logs;

    public long TraceIdLow { get; set; }

    public long TraceIdHigh { get; set; }

    public long SpanId { get; set; }

    public long ParentSpanId { get; set; }

    public string OperationName { get; set; }

    public List<global::Jaeger.Thrift.SpanRef> References
    {
      get
      {
        return _references;
      }
      set
      {
        __isset.references = true;
        this._references = value;
      }
    }

    public int Flags { get; set; }

    public long StartTime { get; set; }

    public long Duration { get; set; }

    public List<global::Jaeger.Thrift.Tag> Tags
    {
      get
      {
        return _tags;
      }
      set
      {
        __isset.tags = true;
        this._tags = value;
      }
    }

    public List<global::Jaeger.Thrift.Log> Logs
    {
      get
      {
        return _logs;
      }
      set
      {
        __isset.logs = true;
        this._logs = value;
      }
    }


    public Isset __isset;
    public struct Isset
    {
      public bool references;
      public bool tags;
      public bool logs;
    }

    public Span()
    {
    }

    public Span(long traceIdLow, long traceIdHigh, long spanId, long parentSpanId, string operationName, int flags, long startTime, long duration) : this()
    {
      this.TraceIdLow = traceIdLow;
      this.TraceIdHigh = traceIdHigh;
      this.SpanId = spanId;
      this.ParentSpanId = parentSpanId;
      this.OperationName = operationName;
      this.Flags = flags;
      this.StartTime = startTime;
      this.Duration = duration;
    }

    public Span DeepCopy()
    {
      var tmp10 = new Span();
      tmp10.TraceIdLow = this.TraceIdLow;
      tmp10.TraceIdHigh = this.TraceIdHigh;
      tmp10.SpanId = this.SpanId;
      tmp10.ParentSpanId = this.ParentSpanId;
      if((OperationName != null))
      {
        tmp10.OperationName = this.OperationName;
      }
      if((References != null) && __isset.references)
      {
        tmp10.References = this.References.DeepCopy();
      }
      tmp10.__isset.references = this.__isset.references;
      tmp10.Flags = this.Flags;
      tmp10.StartTime = this.StartTime;
      tmp10.Duration = this.Duration;
      if((Tags != null) && __isset.tags)
      {
        tmp10.Tags = this.Tags.DeepCopy();
      }
      tmp10.__isset.tags = this.__isset.tags;
      if((Logs != null) && __isset.logs)
      {
        tmp10.Logs = this.Logs.DeepCopy();
      }
      tmp10.__isset.logs = this.__isset.logs;
      return tmp10;
    }

    public async global::System.Threading.Tasks.Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
    {
      iprot.IncrementRecursionDepth();
      try
      {
        bool isset_traceIdLow = false;
        bool isset_traceIdHigh = false;
        bool isset_spanId = false;
        bool isset_parentSpanId = false;
        bool isset_operationName = false;
        bool isset_flags = false;
        bool isset_startTime = false;
        bool isset_duration = false;
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
              if (field.Type == TType.I64)
              {
                TraceIdLow = await iprot.ReadI64Async(cancellationToken);
                isset_traceIdLow = true;
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 2:
              if (field.Type == TType.I64)
              {
                TraceIdHigh = await iprot.ReadI64Async(cancellationToken);
                isset_traceIdHigh = true;
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 3:
              if (field.Type == TType.I64)
              {
                SpanId = await iprot.ReadI64Async(cancellationToken);
                isset_spanId = true;
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 4:
              if (field.Type == TType.I64)
              {
                ParentSpanId = await iprot.ReadI64Async(cancellationToken);
                isset_parentSpanId = true;
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 5:
              if (field.Type == TType.String)
              {
                OperationName = await iprot.ReadStringAsync(cancellationToken);
                isset_operationName = true;
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 6:
              if (field.Type == TType.List)
              {
                {
                  TList _list11 = await iprot.ReadListBeginAsync(cancellationToken);
                  References = new List<global::Jaeger.Thrift.SpanRef>(_list11.Count);
                  for(int _i12 = 0; _i12 < _list11.Count; ++_i12)
                  {
                    global::Jaeger.Thrift.SpanRef _elem13;
                    _elem13 = new global::Jaeger.Thrift.SpanRef();
                    await _elem13.ReadAsync(iprot, cancellationToken);
                    References.Add(_elem13);
                  }
                  await iprot.ReadListEndAsync(cancellationToken);
                }
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 7:
              if (field.Type == TType.I32)
              {
                Flags = await iprot.ReadI32Async(cancellationToken);
                isset_flags = true;
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 8:
              if (field.Type == TType.I64)
              {
                StartTime = await iprot.ReadI64Async(cancellationToken);
                isset_startTime = true;
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 9:
              if (field.Type == TType.I64)
              {
                Duration = await iprot.ReadI64Async(cancellationToken);
                isset_duration = true;
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 10:
              if (field.Type == TType.List)
              {
                {
                  TList _list14 = await iprot.ReadListBeginAsync(cancellationToken);
                  Tags = new List<global::Jaeger.Thrift.Tag>(_list14.Count);
                  for(int _i15 = 0; _i15 < _list14.Count; ++_i15)
                  {
                    global::Jaeger.Thrift.Tag _elem16;
                    _elem16 = new global::Jaeger.Thrift.Tag();
                    await _elem16.ReadAsync(iprot, cancellationToken);
                    Tags.Add(_elem16);
                  }
                  await iprot.ReadListEndAsync(cancellationToken);
                }
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 11:
              if (field.Type == TType.List)
              {
                {
                  TList _list17 = await iprot.ReadListBeginAsync(cancellationToken);
                  Logs = new List<global::Jaeger.Thrift.Log>(_list17.Count);
                  for(int _i18 = 0; _i18 < _list17.Count; ++_i18)
                  {
                    global::Jaeger.Thrift.Log _elem19;
                    _elem19 = new global::Jaeger.Thrift.Log();
                    await _elem19.ReadAsync(iprot, cancellationToken);
                    Logs.Add(_elem19);
                  }
                  await iprot.ReadListEndAsync(cancellationToken);
                }
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
        if (!isset_traceIdLow)
        {
          throw new TProtocolException(TProtocolException.INVALID_DATA);
        }
        if (!isset_traceIdHigh)
        {
          throw new TProtocolException(TProtocolException.INVALID_DATA);
        }
        if (!isset_spanId)
        {
          throw new TProtocolException(TProtocolException.INVALID_DATA);
        }
        if (!isset_parentSpanId)
        {
          throw new TProtocolException(TProtocolException.INVALID_DATA);
        }
        if (!isset_operationName)
        {
          throw new TProtocolException(TProtocolException.INVALID_DATA);
        }
        if (!isset_flags)
        {
          throw new TProtocolException(TProtocolException.INVALID_DATA);
        }
        if (!isset_startTime)
        {
          throw new TProtocolException(TProtocolException.INVALID_DATA);
        }
        if (!isset_duration)
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
        var struc = new TStruct("Span");
        await oprot.WriteStructBeginAsync(struc, cancellationToken);
        var field = new TField();
        field.Name = "traceIdLow";
        field.Type = TType.I64;
        field.ID = 1;
        await oprot.WriteFieldBeginAsync(field, cancellationToken);
        await oprot.WriteI64Async(TraceIdLow, cancellationToken);
        await oprot.WriteFieldEndAsync(cancellationToken);
        field.Name = "traceIdHigh";
        field.Type = TType.I64;
        field.ID = 2;
        await oprot.WriteFieldBeginAsync(field, cancellationToken);
        await oprot.WriteI64Async(TraceIdHigh, cancellationToken);
        await oprot.WriteFieldEndAsync(cancellationToken);
        field.Name = "spanId";
        field.Type = TType.I64;
        field.ID = 3;
        await oprot.WriteFieldBeginAsync(field, cancellationToken);
        await oprot.WriteI64Async(SpanId, cancellationToken);
        await oprot.WriteFieldEndAsync(cancellationToken);
        field.Name = "parentSpanId";
        field.Type = TType.I64;
        field.ID = 4;
        await oprot.WriteFieldBeginAsync(field, cancellationToken);
        await oprot.WriteI64Async(ParentSpanId, cancellationToken);
        await oprot.WriteFieldEndAsync(cancellationToken);
        if((OperationName != null))
        {
          field.Name = "operationName";
          field.Type = TType.String;
          field.ID = 5;
          await oprot.WriteFieldBeginAsync(field, cancellationToken);
          await oprot.WriteStringAsync(OperationName, cancellationToken);
          await oprot.WriteFieldEndAsync(cancellationToken);
        }
        if((References != null) && __isset.references)
        {
          field.Name = "references";
          field.Type = TType.List;
          field.ID = 6;
          await oprot.WriteFieldBeginAsync(field, cancellationToken);
          {
            await oprot.WriteListBeginAsync(new TList(TType.Struct, References.Count), cancellationToken);
            foreach (global::Jaeger.Thrift.SpanRef _iter20 in References)
            {
              await _iter20.WriteAsync(oprot, cancellationToken);
            }
            await oprot.WriteListEndAsync(cancellationToken);
          }
          await oprot.WriteFieldEndAsync(cancellationToken);
        }
        field.Name = "flags";
        field.Type = TType.I32;
        field.ID = 7;
        await oprot.WriteFieldBeginAsync(field, cancellationToken);
        await oprot.WriteI32Async(Flags, cancellationToken);
        await oprot.WriteFieldEndAsync(cancellationToken);
        field.Name = "startTime";
        field.Type = TType.I64;
        field.ID = 8;
        await oprot.WriteFieldBeginAsync(field, cancellationToken);
        await oprot.WriteI64Async(StartTime, cancellationToken);
        await oprot.WriteFieldEndAsync(cancellationToken);
        field.Name = "duration";
        field.Type = TType.I64;
        field.ID = 9;
        await oprot.WriteFieldBeginAsync(field, cancellationToken);
        await oprot.WriteI64Async(Duration, cancellationToken);
        await oprot.WriteFieldEndAsync(cancellationToken);
        if((Tags != null) && __isset.tags)
        {
          field.Name = "tags";
          field.Type = TType.List;
          field.ID = 10;
          await oprot.WriteFieldBeginAsync(field, cancellationToken);
          {
            await oprot.WriteListBeginAsync(new TList(TType.Struct, Tags.Count), cancellationToken);
            foreach (global::Jaeger.Thrift.Tag _iter21 in Tags)
            {
              await _iter21.WriteAsync(oprot, cancellationToken);
            }
            await oprot.WriteListEndAsync(cancellationToken);
          }
          await oprot.WriteFieldEndAsync(cancellationToken);
        }
        if((Logs != null) && __isset.logs)
        {
          field.Name = "logs";
          field.Type = TType.List;
          field.ID = 11;
          await oprot.WriteFieldBeginAsync(field, cancellationToken);
          {
            await oprot.WriteListBeginAsync(new TList(TType.Struct, Logs.Count), cancellationToken);
            foreach (global::Jaeger.Thrift.Log _iter22 in Logs)
            {
              await _iter22.WriteAsync(oprot, cancellationToken);
            }
            await oprot.WriteListEndAsync(cancellationToken);
          }
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
      if (!(that is Span other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return System.Object.Equals(TraceIdLow, other.TraceIdLow)
        && System.Object.Equals(TraceIdHigh, other.TraceIdHigh)
        && System.Object.Equals(SpanId, other.SpanId)
        && System.Object.Equals(ParentSpanId, other.ParentSpanId)
        && System.Object.Equals(OperationName, other.OperationName)
        && ((__isset.references == other.__isset.references) && ((!__isset.references) || (TCollections.Equals(References, other.References))))
        && System.Object.Equals(Flags, other.Flags)
        && System.Object.Equals(StartTime, other.StartTime)
        && System.Object.Equals(Duration, other.Duration)
        && ((__isset.tags == other.__isset.tags) && ((!__isset.tags) || (TCollections.Equals(Tags, other.Tags))))
        && ((__isset.logs == other.__isset.logs) && ((!__isset.logs) || (TCollections.Equals(Logs, other.Logs))));
    }

    public override int GetHashCode() {
      int hashcode = 157;
      unchecked {
        hashcode = (hashcode * 397) + TraceIdLow.GetHashCode();
        hashcode = (hashcode * 397) + TraceIdHigh.GetHashCode();
        hashcode = (hashcode * 397) + SpanId.GetHashCode();
        hashcode = (hashcode * 397) + ParentSpanId.GetHashCode();
        if((OperationName != null))
        {
          hashcode = (hashcode * 397) + OperationName.GetHashCode();
        }
        if((References != null) && __isset.references)
        {
          hashcode = (hashcode * 397) + TCollections.GetHashCode(References);
        }
        hashcode = (hashcode * 397) + Flags.GetHashCode();
        hashcode = (hashcode * 397) + StartTime.GetHashCode();
        hashcode = (hashcode * 397) + Duration.GetHashCode();
        if((Tags != null) && __isset.tags)
        {
          hashcode = (hashcode * 397) + TCollections.GetHashCode(Tags);
        }
        if((Logs != null) && __isset.logs)
        {
          hashcode = (hashcode * 397) + TCollections.GetHashCode(Logs);
        }
      }
      return hashcode;
    }

    public override string ToString()
    {
      var sb = new StringBuilder("Span(");
      sb.Append(", TraceIdLow: ");
      TraceIdLow.ToString(sb);
      sb.Append(", TraceIdHigh: ");
      TraceIdHigh.ToString(sb);
      sb.Append(", SpanId: ");
      SpanId.ToString(sb);
      sb.Append(", ParentSpanId: ");
      ParentSpanId.ToString(sb);
      if((OperationName != null))
      {
        sb.Append(", OperationName: ");
        OperationName.ToString(sb);
      }
      if((References != null) && __isset.references)
      {
        sb.Append(", References: ");
        References.ToString(sb);
      }
      sb.Append(", Flags: ");
      Flags.ToString(sb);
      sb.Append(", StartTime: ");
      StartTime.ToString(sb);
      sb.Append(", Duration: ");
      Duration.ToString(sb);
      if((Tags != null) && __isset.tags)
      {
        sb.Append(", Tags: ");
        Tags.ToString(sb);
      }
      if((Logs != null) && __isset.logs)
      {
        sb.Append(", Logs: ");
        Logs.ToString(sb);
      }
      sb.Append(')');
      return sb.ToString();
    }
  }

}
