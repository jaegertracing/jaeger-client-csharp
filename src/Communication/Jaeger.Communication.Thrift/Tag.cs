/**
 * Autogenerated by Thrift Compiler (0.14.1)
 *
 * DO NOT EDIT UNLESS YOU ARE SURE THAT YOU KNOW WHAT YOU ARE DOING
 *  @generated
 */

using System.Text;
using System.Linq;
using System.Threading;
using Thrift.Collections;

using Thrift.Protocol;
using Thrift.Protocol.Entities;
using Thrift.Protocol.Utilities;

#pragma warning disable IDE0079  // remove unnecessary pragmas
#pragma warning disable IDE1006  // parts of the code use IDL spelling

namespace Jaeger.Thrift
{

  public partial class Tag : TBase
  {
    private string _vStr;
    private double _vDouble;
    private bool _vBool;
    private long _vLong;
    private byte[] _vBinary;

    public string Key { get; set; }

    /// <summary>
    /// 
    /// <seealso cref="global::Jaeger.Thrift.TagType"/>
    /// </summary>
    public global::Jaeger.Thrift.TagType VType { get; set; }

    public string VStr
    {
      get
      {
        return _vStr;
      }
      set
      {
        __isset.vStr = true;
        this._vStr = value;
      }
    }

    public double VDouble
    {
      get
      {
        return _vDouble;
      }
      set
      {
        __isset.vDouble = true;
        this._vDouble = value;
      }
    }

    public bool VBool
    {
      get
      {
        return _vBool;
      }
      set
      {
        __isset.vBool = true;
        this._vBool = value;
      }
    }

    public long VLong
    {
      get
      {
        return _vLong;
      }
      set
      {
        __isset.vLong = true;
        this._vLong = value;
      }
    }

    public byte[] VBinary
    {
      get
      {
        return _vBinary;
      }
      set
      {
        __isset.vBinary = true;
        this._vBinary = value;
      }
    }


    public Isset __isset;
    public struct Isset
    {
      public bool vStr;
      public bool vDouble;
      public bool vBool;
      public bool vLong;
      public bool vBinary;
    }

    public Tag()
    {
    }

    public Tag(string key, global::Jaeger.Thrift.TagType vType) : this()
    {
      this.Key = key;
      this.VType = vType;
    }

    public Tag DeepCopy()
    {
      var tmp0 = new Tag();
      if((Key != null))
      {
        tmp0.Key = this.Key;
      }
      tmp0.VType = this.VType;
      if((VStr != null) && __isset.vStr)
      {
        tmp0.VStr = this.VStr;
      }
      tmp0.__isset.vStr = this.__isset.vStr;
      if(__isset.vDouble)
      {
        tmp0.VDouble = this.VDouble;
      }
      tmp0.__isset.vDouble = this.__isset.vDouble;
      if(__isset.vBool)
      {
        tmp0.VBool = this.VBool;
      }
      tmp0.__isset.vBool = this.__isset.vBool;
      if(__isset.vLong)
      {
        tmp0.VLong = this.VLong;
      }
      tmp0.__isset.vLong = this.__isset.vLong;
      if((VBinary != null) && __isset.vBinary)
      {
        tmp0.VBinary = this.VBinary.ToArray();
      }
      tmp0.__isset.vBinary = this.__isset.vBinary;
      return tmp0;
    }

    public async global::System.Threading.Tasks.Task ReadAsync(TProtocol iprot, CancellationToken cancellationToken)
    {
      iprot.IncrementRecursionDepth();
      try
      {
        bool isset_key = false;
        bool isset_vType = false;
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
                Key = await iprot.ReadStringAsync(cancellationToken);
                isset_key = true;
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 2:
              if (field.Type == TType.I32)
              {
                VType = (global::Jaeger.Thrift.TagType)await iprot.ReadI32Async(cancellationToken);
                isset_vType = true;
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 3:
              if (field.Type == TType.String)
              {
                VStr = await iprot.ReadStringAsync(cancellationToken);
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 4:
              if (field.Type == TType.Double)
              {
                VDouble = await iprot.ReadDoubleAsync(cancellationToken);
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 5:
              if (field.Type == TType.Bool)
              {
                VBool = await iprot.ReadBoolAsync(cancellationToken);
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 6:
              if (field.Type == TType.I64)
              {
                VLong = await iprot.ReadI64Async(cancellationToken);
              }
              else
              {
                await TProtocolUtil.SkipAsync(iprot, field.Type, cancellationToken);
              }
              break;
            case 7:
              if (field.Type == TType.String)
              {
                VBinary = await iprot.ReadBinaryAsync(cancellationToken);
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
        if (!isset_key)
        {
          throw new TProtocolException(TProtocolException.INVALID_DATA);
        }
        if (!isset_vType)
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
        var struc = new TStruct("Tag");
        await oprot.WriteStructBeginAsync(struc, cancellationToken);
        var field = new TField();
        if((Key != null))
        {
          field.Name = "key";
          field.Type = TType.String;
          field.ID = 1;
          await oprot.WriteFieldBeginAsync(field, cancellationToken);
          await oprot.WriteStringAsync(Key, cancellationToken);
          await oprot.WriteFieldEndAsync(cancellationToken);
        }
        field.Name = "vType";
        field.Type = TType.I32;
        field.ID = 2;
        await oprot.WriteFieldBeginAsync(field, cancellationToken);
        await oprot.WriteI32Async((int)VType, cancellationToken);
        await oprot.WriteFieldEndAsync(cancellationToken);
        if((VStr != null) && __isset.vStr)
        {
          field.Name = "vStr";
          field.Type = TType.String;
          field.ID = 3;
          await oprot.WriteFieldBeginAsync(field, cancellationToken);
          await oprot.WriteStringAsync(VStr, cancellationToken);
          await oprot.WriteFieldEndAsync(cancellationToken);
        }
        if(__isset.vDouble)
        {
          field.Name = "vDouble";
          field.Type = TType.Double;
          field.ID = 4;
          await oprot.WriteFieldBeginAsync(field, cancellationToken);
          await oprot.WriteDoubleAsync(VDouble, cancellationToken);
          await oprot.WriteFieldEndAsync(cancellationToken);
        }
        if(__isset.vBool)
        {
          field.Name = "vBool";
          field.Type = TType.Bool;
          field.ID = 5;
          await oprot.WriteFieldBeginAsync(field, cancellationToken);
          await oprot.WriteBoolAsync(VBool, cancellationToken);
          await oprot.WriteFieldEndAsync(cancellationToken);
        }
        if(__isset.vLong)
        {
          field.Name = "vLong";
          field.Type = TType.I64;
          field.ID = 6;
          await oprot.WriteFieldBeginAsync(field, cancellationToken);
          await oprot.WriteI64Async(VLong, cancellationToken);
          await oprot.WriteFieldEndAsync(cancellationToken);
        }
        if((VBinary != null) && __isset.vBinary)
        {
          field.Name = "vBinary";
          field.Type = TType.String;
          field.ID = 7;
          await oprot.WriteFieldBeginAsync(field, cancellationToken);
          await oprot.WriteBinaryAsync(VBinary, cancellationToken);
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
      if (!(that is Tag other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return System.Object.Equals(Key, other.Key)
        && System.Object.Equals(VType, other.VType)
        && ((__isset.vStr == other.__isset.vStr) && ((!__isset.vStr) || (System.Object.Equals(VStr, other.VStr))))
        && ((__isset.vDouble == other.__isset.vDouble) && ((!__isset.vDouble) || (System.Object.Equals(VDouble, other.VDouble))))
        && ((__isset.vBool == other.__isset.vBool) && ((!__isset.vBool) || (System.Object.Equals(VBool, other.VBool))))
        && ((__isset.vLong == other.__isset.vLong) && ((!__isset.vLong) || (System.Object.Equals(VLong, other.VLong))))
        && ((__isset.vBinary == other.__isset.vBinary) && ((!__isset.vBinary) || (TCollections.Equals(VBinary, other.VBinary))));
    }

    public override int GetHashCode() {
      int hashcode = 157;
      unchecked {
        if((Key != null))
        {
          hashcode = (hashcode * 397) + Key.GetHashCode();
        }
        hashcode = (hashcode * 397) + VType.GetHashCode();
        if((VStr != null) && __isset.vStr)
        {
          hashcode = (hashcode * 397) + VStr.GetHashCode();
        }
        if(__isset.vDouble)
        {
          hashcode = (hashcode * 397) + VDouble.GetHashCode();
        }
        if(__isset.vBool)
        {
          hashcode = (hashcode * 397) + VBool.GetHashCode();
        }
        if(__isset.vLong)
        {
          hashcode = (hashcode * 397) + VLong.GetHashCode();
        }
        if((VBinary != null) && __isset.vBinary)
        {
          hashcode = (hashcode * 397) + VBinary.GetHashCode();
        }
      }
      return hashcode;
    }

    public override string ToString()
    {
      var sb = new StringBuilder("Tag(");
      if((Key != null))
      {
        sb.Append(", Key: ");
        Key.ToString(sb);
      }
      sb.Append(", VType: ");
      VType.ToString(sb);
      if((VStr != null) && __isset.vStr)
      {
        sb.Append(", VStr: ");
        VStr.ToString(sb);
      }
      if(__isset.vDouble)
      {
        sb.Append(", VDouble: ");
        VDouble.ToString(sb);
      }
      if(__isset.vBool)
      {
        sb.Append(", VBool: ");
        VBool.ToString(sb);
      }
      if(__isset.vLong)
      {
        sb.Append(", VLong: ");
        VLong.ToString(sb);
      }
      if((VBinary != null) && __isset.vBinary)
      {
        sb.Append(", VBinary: ");
        VBinary.ToString(sb);
      }
      sb.Append(')');
      return sb.ToString();
    }
  }

}
