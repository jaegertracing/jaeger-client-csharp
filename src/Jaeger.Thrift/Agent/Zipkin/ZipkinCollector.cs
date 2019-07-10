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


namespace Jaeger.Thrift.Agent.Zipkin
{
  public partial class ZipkinCollector
  {
    public interface IAsync
    {
      Task<List<Response>> submitZipkinBatchAsync(List<Span> spans, CancellationToken cancellationToken);

    }


    public class Client : TBaseClient, IDisposable, IAsync
    {
      public Client(TProtocol protocol) : this(protocol, protocol)
      {
      }

      public Client(TProtocol inputProtocol, TProtocol outputProtocol) : base(inputProtocol, outputProtocol)      {
      }
      public async Task<List<Response>> submitZipkinBatchAsync(List<Span> spans, CancellationToken cancellationToken)
      {
        await OutputProtocol.WriteMessageBeginAsync(new TMessage("submitZipkinBatch", TMessageType.Call, SeqId), cancellationToken);
        
        var args = new submitZipkinBatchArgs();
        args.Spans = spans;
        
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

        var result = new submitZipkinBatchResult();
        await result.ReadAsync(InputProtocol, cancellationToken);
        await InputProtocol.ReadMessageEndAsync(cancellationToken);
        if (result.__isset.success)
        {
          return result.Success;
        }
        throw new TApplicationException(TApplicationException.ExceptionType.MissingResult, "submitZipkinBatch failed: unknown result");
      }

    }

    public class AsyncProcessor : ITAsyncProcessor
    {
      private IAsync _iAsync;

      public AsyncProcessor(IAsync iAsync)
      {
        if (iAsync == null) throw new ArgumentNullException(nameof(iAsync));

        _iAsync = iAsync;
        processMap_["submitZipkinBatch"] = submitZipkinBatch_ProcessAsync;
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

      public async Task submitZipkinBatch_ProcessAsync(int seqid, TProtocol iprot, TProtocol oprot, CancellationToken cancellationToken)
      {
        var args = new submitZipkinBatchArgs();
        await args.ReadAsync(iprot, cancellationToken);
        await iprot.ReadMessageEndAsync(cancellationToken);
        var result = new submitZipkinBatchResult();
        try
        {
          result.Success = await _iAsync.submitZipkinBatchAsync(args.Spans, cancellationToken);
          await oprot.WriteMessageBeginAsync(new TMessage("submitZipkinBatch", TMessageType.Reply, seqid), cancellationToken); 
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
          await oprot.WriteMessageBeginAsync(new TMessage("submitZipkinBatch", TMessageType.Exception, seqid), cancellationToken);
          await x.WriteAsync(oprot, cancellationToken);
        }
        await oprot.WriteMessageEndAsync(cancellationToken);
        await oprot.Transport.FlushAsync(cancellationToken);
      }

    }


    public partial class submitZipkinBatchArgs : TBase
    {
      private List<Span> _spans;

      public List<Span> Spans
      {
        get
        {
          return _spans;
        }
        set
        {
          __isset.spans = true;
          this._spans = value;
        }
      }


      public Isset __isset;
      public struct Isset
      {
        public bool spans;
      }

      public submitZipkinBatchArgs()
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
              case 1:
                if (field.Type == TType.List)
                {
                  {
                    Spans = new List<Span>();
                    TList _list8 = await iprot.ReadListBeginAsync(cancellationToken);
                    for(int _i9 = 0; _i9 < _list8.Count; ++_i9)
                    {
                      Span _elem10;
                      _elem10 = new Span();
                      await _elem10.ReadAsync(iprot, cancellationToken);
                      Spans.Add(_elem10);
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
          var struc = new TStruct("submitZipkinBatch_args");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();
          if (Spans != null && __isset.spans)
          {
            field.Name = "spans";
            field.Type = TType.List;
            field.ID = 1;
            await oprot.WriteFieldBeginAsync(field, cancellationToken);
            {
              await oprot.WriteListBeginAsync(new TList(TType.Struct, Spans.Count), cancellationToken);
              foreach (Span _iter11 in Spans)
              {
                await _iter11.WriteAsync(oprot, cancellationToken);
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

      public override string ToString()
      {
        var sb = new StringBuilder("submitZipkinBatch_args(");
        bool __first = true;
        if (Spans != null && __isset.spans)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Spans: ");
          sb.Append(Spans);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }


    public partial class submitZipkinBatchResult : TBase
    {
      private List<Response> _success;

      public List<Response> Success
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

      public submitZipkinBatchResult()
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
                if (field.Type == TType.List)
                {
                  {
                    Success = new List<Response>();
                    TList _list12 = await iprot.ReadListBeginAsync(cancellationToken);
                    for(int _i13 = 0; _i13 < _list12.Count; ++_i13)
                    {
                      Response _elem14;
                      _elem14 = new Response();
                      await _elem14.ReadAsync(iprot, cancellationToken);
                      Success.Add(_elem14);
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
          var struc = new TStruct("submitZipkinBatch_result");
          await oprot.WriteStructBeginAsync(struc, cancellationToken);
          var field = new TField();

          if(this.__isset.success)
          {
            if (Success != null)
            {
              field.Name = "Success";
              field.Type = TType.List;
              field.ID = 0;
              await oprot.WriteFieldBeginAsync(field, cancellationToken);
              {
                await oprot.WriteListBeginAsync(new TList(TType.Struct, Success.Count), cancellationToken);
                foreach (Response _iter15 in Success)
                {
                  await _iter15.WriteAsync(oprot, cancellationToken);
                }
                await oprot.WriteListEndAsync(cancellationToken);
              }
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

      public override string ToString()
      {
        var sb = new StringBuilder("submitZipkinBatch_result(");
        bool __first = true;
        if (Success != null && __isset.success)
        {
          if(!__first) { sb.Append(", "); }
          __first = false;
          sb.Append("Success: ");
          sb.Append(Success);
        }
        sb.Append(")");
        return sb.ToString();
      }
    }

  }
}
