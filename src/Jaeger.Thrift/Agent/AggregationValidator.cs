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
  public partial class AggregationValidator
  {
    public interface IAsync
    {
      Task<ValidateTraceResponse> validateTraceAsync(string traceId, CancellationToken cancellationToken = default(CancellationToken));

    }


    public class Client : TBaseClient, IDisposable, IAsync
    {
      public Client(TProtocol protocol) : this(protocol, protocol)
      {
      }

      public Client(TProtocol inputProtocol, TProtocol outputProtocol) : base(inputProtocol, outputProtocol)      {
      }
      public async Task<ValidateTraceResponse> validateTraceAsync(string traceId, CancellationToken cancellationToken = default(CancellationToken))
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("validateTrace", TMessageType.Call, SeqId), cancellationToken);
        
        var args = new validateTraceArgs();
        args.TraceId = traceId;
        
        await args.WriteAsync(OutputProtocol, cancellationToken);
        await OutputProtocol.WriteMessageEndAsync(cancellationToken);
        await OutputProtocol.Transport.FlushAsync(cancellationToken);
        
        var msg = await InputProtocol.ReadMessageBeginAsync(cancellationToken);
        if (msg.Type == TMessageType.Exception)
        {
          var x = await TApplicationException.ReadAsync(InputProtocol, cancellationToken);
          await InputProtocol.ReadMessageEndAsync(cancellationToken);
          throw x;
        }

        var result = new validateTraceResult();
        await result.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (result.__isset.success)
        {
          return result.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "validateTrace failed: unknown result");
      }

    }

    public class AsyncProcessor : ITAsyncProcessor
    {
      private IAsync _iAsync;

      public AsyncProcessor(IAsync iAsync)
      {
        if (iAsync == null) throw new ArgumentNullException(nameof(iAsync));

        _iAsync = iAsync;
        processMap_["validateTrace"] = validateTrace_ProcessAsync;
      }

      protected delegate Task ProcessFunction(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken);
      protected Dictionary<string, ProcessFunction> processMap_ = new Dictionary<string, ProcessFunction>();

      public async Task<bool> ProcessAsync(TProtocol iprot, TProtocol oprot)
      {
        return await ProcessAsync(iprot, oprot, CancellationToken.None);
      }

      public async Task<bool> ProcessAsync(TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        try
        {
          var msg = await iprot.ReadMessageBeginAsync(cancellationToken);

          ProcessFunction fn;
          processMap_.TryGetValue(msg.Name, out fn);

          if (fn == null)
          {
            await TProtocolUtil.SkipAsync(iprot, TType.Struct, cancellationToken);
            await iprot.ReadMessageEndAsync(cancellationToken);
            var x = new TApplicationException (TApplicationException.ExceptionType.UnknownMethod, "Invalid method name: '" + msg.Name + "'");
            await oprot.WriteMessageBeginAsync(new TMessage(msg.Name, TMessageType.Exception, msg.SeqID), cancellationToken);
            await x.WriteAsync(oprot, cancellationToken);
            await oprot.WriteMessageEndAsync(cancellationToken);
            await oprot.Transport.FlushAsync(cancellationToken);
            return true;
          }

          await fn(msg.SeqID, iprot, oprot, cancellationToken);

        }
        catch (IOException)
        {
          return false;
        }

        return true;
      }

      public async Task validateTrace_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new validateTraceArgs();
        await args.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var result = new validateTraceResult();
        try
        {
          result.Success = await _iAsync.validateTraceAsync(args.TraceId, cancellationToken);
          await oprot.WriteMessageBeginAsync(new TMessage("validateTrace", TMessageType.Reply, seqid), cancellationToken); 
          await result.WriteAsync(oprot, cancellationToken);
        }
        catch (TTransportException)
        {
          throw;
        }
        catch (Exception ex)
        {
          Console.Error.WriteLine("Error occurred in processor:");
          Console.Error.WriteLine(ex.ToString());
          var x = new TApplicationException(TApplicationException.ExceptionType.InternalError," Internal error.");
          await oprot.WriteMessageBeginAsync(new TMessage("validateTrace", TMessageType.Exception, seqid), cancellationToken);
          await x.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

    }


    public partial class validateTraceArgs : TBase
    {

      public string TraceId { get; set; }

      public validateTraceArgs()
      {
      }

      public validateTraceArgs(string traceId) : this()
      {
        this.TraceId = traceId;
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
          bool isset_traceId = false;
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
                  TraceId = await iprot.ReadStringAsync(cancellationToken);
                  isset_traceId = true;
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
          if (!isset_traceId)
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
          var struc = new TStruct("validateTrace_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();
          field.Name = "traceId";
          field.Type = TType.String;
          field.ID = 1;
          await oprot.WriteFieldBeginAsync(field, cancellationToken);
          await oprot.WriteStringAsync(TraceId, cancellationToken);
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
        var other = that as validateTraceArgs;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return System.Object.Equals(TraceId, other.TraceId);
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          hashcode = (hashcode * 397) + TraceId.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("validateTrace_args(");
        sb.Append(", TraceId: ");
        sb.Append(TraceId);
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class validateTraceResult : TBase
    {
      private ValidateTraceResponse _success;

      public ValidateTraceResponse Success
      {
        get
        {
          return _success;
        }
        set
        {
          __isset.success = true;
          this._success = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool success;
      }

      public validateTraceResult()
      {
      }

      public async Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
      {
        iprot.IncrementRecursionDepth();
        try
        {
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
              case 0:
                if (field.Type == TType.Struct)
                {
                  Success = new ValidateTraceResponse();
                  await Success.ReadAsync(iprot, cancellationToken);
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
          var struc = new TStruct("validateTrace_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();

          if(this.__isset.success)
          {
            if (Success != null)
            {
              field.Name = "Success";
              field.Type = TType.Struct;
              field.ID = 0;
              await oprot.WriteFieldBeginAsync(field, cancellationToken);
              await Success.WriteAsync(oprot, cancellationToken);
              await oprot.WriteFieldEndAsync(cancellationToken);
            }
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
        var other = that as validateTraceResult;
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ((__isset.success == other.__isset.success) && ((!__isset.success) || (System.Object.Equals(Success, other.Success))));
      }

      public override int GetHashCode() {
        int hashcode = 157;
        unchecked {
          if(__isset.success)
            hashcode = (hashcode * 397) + Success.GetHashCode();
        }
        return hashcode;
      }

      public override string ToString()
      {
        var sb = new StringBuilder("validateTrace_result(");
        bool __first = true;
        if (Success != null && __isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success== null ? "<null>" : Success.ToString());
        }
        sb.Append(")");
        return sb.ToString();
      }
    }

  }
}
