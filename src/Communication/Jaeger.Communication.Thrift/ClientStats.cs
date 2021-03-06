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

namespace Jaeger.Thrift
{

  public partial class ClientStats : TBase
  {

    public long FullQueueDroppedSpans { get; set; }

    public long TooLargeDroppedSpans { get; set; }

    public long FailedToEmitSpans { get; set; }

    public ClientStats()
    {
    }

    public ClientStats(long fullQueueDroppedSpans, long tooLargeDroppedSpans, long failedToEmitSpans) : this()
    {
      this.FullQueueDroppedSpans = fullQueueDroppedSpans;
      this.TooLargeDroppedSpans = tooLargeDroppedSpans;
      this.FailedToEmitSpans = failedToEmitSpans;
    }

    public ClientStats DeepCopy()
    {
      var tmp30 = new ClientStats();
      tmp30.FullQueueDroppedSpans = this.FullQueueDroppedSpans;
      tmp30.TooLargeDroppedSpans = this.TooLargeDroppedSpans;
      tmp30.FailedToEmitSpans = this.FailedToEmitSpans;
      return tmp30;
    }

    public async global::System.Threading.Tasks.Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
    {
      iprot.IncrementRecursionDepth();
      try
      {
        bool isset_fullQueueDroppedSpans = false;
        bool isset_tooLargeDroppedSpans = false;
        bool isset_failedToEmitSpans = false;
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
                FullQueueDroppedSpans = await iprot.ReadI64Async(cancellationToken);
                isset_fullQueueDroppedSpans = true;
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 2:
              if (field.Type == TType.I64)
              {
                TooLargeDroppedSpans = await iprot.ReadI64Async(cancellationToken);
                isset_tooLargeDroppedSpans = true;
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 3:
              if (field.Type == TType.I64)
              {
                FailedToEmitSpans = await iprot.ReadI64Async(cancellationToken);
                isset_failedToEmitSpans = true;
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
        if (!isset_fullQueueDroppedSpans)
        {
          throw new TProtocolException(TProtocolException.INVALID_DATA);
        }
        if (!isset_tooLargeDroppedSpans)
        {
          throw new TProtocolException(TProtocolException.INVALID_DATA);
        }
        if (!isset_failedToEmitSpans)
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
        var struc = new TStruct("ClientStats");
        await oprot.WriteStructBeginAsync(struc, cancellationToken);
        var field = new TField();
        field.Name = "fullQueueDroppedSpans";
        field.Type = TType.I64;
        field.ID = 1;
        await oprot.WriteFieldBeginAsync(field, cancellationToken);
        await oprot.WriteI64Async(FullQueueDroppedSpans, cancellationToken);
        await oprot.WriteFieldEndAsync(cancellationToken);
        field.Name = "tooLargeDroppedSpans";
        field.Type = TType.I64;
        field.ID = 2;
        await oprot.WriteFieldBeginAsync(field, cancellationToken);
        await oprot.WriteI64Async(TooLargeDroppedSpans, cancellationToken);
        await oprot.WriteFieldEndAsync(cancellationToken);
        field.Name = "failedToEmitSpans";
        field.Type = TType.I64;
        field.ID = 3;
        await oprot.WriteFieldBeginAsync(field, cancellationToken);
        await oprot.WriteI64Async(FailedToEmitSpans, cancellationToken);
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
      if (!(that is ClientStats other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return System.Object.Equals(FullQueueDroppedSpans, other.FullQueueDroppedSpans)
        && System.Object.Equals(TooLargeDroppedSpans, other.TooLargeDroppedSpans)
        && System.Object.Equals(FailedToEmitSpans, other.FailedToEmitSpans);
    }

    public override int GetHashCode() {
      int hashcode = 157;
      unchecked {
        hashcode = (hashcode * 397) + FullQueueDroppedSpans.GetHashCode();
        hashcode = (hashcode * 397) + TooLargeDroppedSpans.GetHashCode();
        hashcode = (hashcode * 397) + FailedToEmitSpans.GetHashCode();
      }
      return hashcode;
    }

    public override string ToString()
    {
      var sb = new StringBuilder("ClientStats(");
      sb.Append(", FullQueueDroppedSpans: ");
      FullQueueDroppedSpans.ToString(sb);
      sb.Append(", TooLargeDroppedSpans: ");
      TooLargeDroppedSpans.ToString(sb);
      sb.Append(", FailedToEmitSpans: ");
      FailedToEmitSpans.ToString(sb);
      sb.Append(')');
      return sb.ToString();
    }
  }

}
