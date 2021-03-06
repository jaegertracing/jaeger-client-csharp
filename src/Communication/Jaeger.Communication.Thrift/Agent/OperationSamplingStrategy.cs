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

namespace Jaeger.Thrift.Agent
{

  public partial class OperationSamplingStrategy : TBase
  {

    public string Operation { get; set; }

    public global::Jaeger.Thrift.Agent.ProbabilisticSamplingStrategy ProbabilisticSampling { get; set; }

    public OperationSamplingStrategy()
    {
    }

    public OperationSamplingStrategy(string operation, global::Jaeger.Thrift.Agent.ProbabilisticSamplingStrategy probabilisticSampling) : this()
    {
      this.Operation = operation;
      this.ProbabilisticSampling = probabilisticSampling;
    }

    public OperationSamplingStrategy DeepCopy()
    {
      var tmp4 = new OperationSamplingStrategy();
      if((Operation != null))
      {
        tmp4.Operation = this.Operation;
      }
      if((ProbabilisticSampling != null))
      {
        tmp4.ProbabilisticSampling = (global::Jaeger.Thrift.Agent.ProbabilisticSamplingStrategy)this.ProbabilisticSampling.DeepCopy();
      }
      return tmp4;
    }

    public async global::System.Threading.Tasks.Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
    {
      iprot.IncrementRecursionDepth();
      try
      {
        bool isset_operation = false;
        bool isset_probabilisticSampling = false;
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
                Operation = await iprot.ReadStringAsync(cancellationToken);
                isset_operation = true;
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 2:
              if (field.Type == TType.Struct)
              {
                ProbabilisticSampling = new global::Jaeger.Thrift.Agent.ProbabilisticSamplingStrategy();
                await ProbabilisticSampling.ReadAsync(iprot, cancellationToken);
                isset_probabilisticSampling = true;
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
        if (!isset_operation)
        {
          throw new TProtocolException(TProtocolException.INVALID_DATA);
        }
        if (!isset_probabilisticSampling)
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
        var struc = new TStruct("OperationSamplingStrategy");
        await oprot.WriteStructBeginAsync(struc, cancellationToken);
        var field = new TField();
        if((Operation != null))
        {
          field.Name = "operation";
          field.Type = TType.String;
          field.ID = 1;
          await oprot.WriteFieldBeginAsync(field, cancellationToken);
          await oprot.WriteStringAsync(Operation, cancellationToken);
          await oprot.WriteFieldEndAsync(cancellationToken);
        }
        if((ProbabilisticSampling != null))
        {
          field.Name = "probabilisticSampling";
          field.Type = TType.Struct;
          field.ID = 2;
          await oprot.WriteFieldBeginAsync(field, cancellationToken);
          await ProbabilisticSampling.WriteAsync(oprot, cancellationToken);
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
      if (!(that is OperationSamplingStrategy other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return System.Object.Equals(Operation, other.Operation)
        && System.Object.Equals(ProbabilisticSampling, other.ProbabilisticSampling);
    }

    public override int GetHashCode() {
      int hashcode = 157;
      unchecked {
        if((Operation != null))
        {
          hashcode = (hashcode * 397) + Operation.GetHashCode();
        }
        if((ProbabilisticSampling != null))
        {
          hashcode = (hashcode * 397) + ProbabilisticSampling.GetHashCode();
        }
      }
      return hashcode;
    }

    public override string ToString()
    {
      var sb = new StringBuilder("OperationSamplingStrategy(");
      if((Operation != null))
      {
        sb.Append(", Operation: ");
        Operation.ToString(sb);
      }
      if((ProbabilisticSampling != null))
      {
        sb.Append(", ProbabilisticSampling: ");
        ProbabilisticSampling.ToString(sb);
      }
      sb.Append(')');
      return sb.ToString();
    }
  }

}
