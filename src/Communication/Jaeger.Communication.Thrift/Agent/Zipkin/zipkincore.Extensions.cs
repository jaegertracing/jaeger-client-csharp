/**
 * Autogenerated by Thrift Compiler (0.14.1)
 *
 * DO NOT EDIT UNLESS YOU ARE SURE THAT YOU KNOW WHAT YOU ARE DOING
 *  @generated
 */

using System.Collections.Generic;
using Thrift.Collections;


#pragma warning disable IDE0079  // remove unnecessary pragmas
#pragma warning disable IDE1006  // parts of the code use IDL spelling

namespace Jaeger.Thrift.Agent.Zipkin
{
  public static class zipkincoreExtensions
  {
    public static bool Equals(this List<global::Jaeger.Thrift.Agent.Zipkin.Annotation> instance, object that)
    {
      if (!(that is List<global::Jaeger.Thrift.Agent.Zipkin.Annotation> other)) return false;
      if (ReferenceEquals(instance, other)) return true;

      return TCollections.Equals(instance, other);
    }


    public static int GetHashCode(this List<global::Jaeger.Thrift.Agent.Zipkin.Annotation> instance)
    {
      return TCollections.GetHashCode(instance);
    }


    public static List<global::Jaeger.Thrift.Agent.Zipkin.Annotation> DeepCopy(this List<global::Jaeger.Thrift.Agent.Zipkin.Annotation> source)
    {
      if (source == null)
        return null;

      var tmp30 = new List<global::Jaeger.Thrift.Agent.Zipkin.Annotation>(source.Count);
      foreach (var elem in source)
        tmp30.Add((elem != null) ? elem.DeepCopy() : null);
      return tmp30;
    }


    public static bool Equals(this List<global::Jaeger.Thrift.Agent.Zipkin.BinaryAnnotation> instance, object that)
    {
      if (!(that is List<global::Jaeger.Thrift.Agent.Zipkin.BinaryAnnotation> other)) return false;
      if (ReferenceEquals(instance, other)) return true;

      return TCollections.Equals(instance, other);
    }


    public static int GetHashCode(this List<global::Jaeger.Thrift.Agent.Zipkin.BinaryAnnotation> instance)
    {
      return TCollections.GetHashCode(instance);
    }


    public static List<global::Jaeger.Thrift.Agent.Zipkin.BinaryAnnotation> DeepCopy(this List<global::Jaeger.Thrift.Agent.Zipkin.BinaryAnnotation> source)
    {
      if (source == null)
        return null;

      var tmp31 = new List<global::Jaeger.Thrift.Agent.Zipkin.BinaryAnnotation>(source.Count);
      foreach (var elem in source)
        tmp31.Add((elem != null) ? elem.DeepCopy() : null);
      return tmp31;
    }


    public static bool Equals(this List<global::Jaeger.Thrift.Agent.Zipkin.Response> instance, object that)
    {
      if (!(that is List<global::Jaeger.Thrift.Agent.Zipkin.Response> other)) return false;
      if (ReferenceEquals(instance, other)) return true;

      return TCollections.Equals(instance, other);
    }


    public static int GetHashCode(this List<global::Jaeger.Thrift.Agent.Zipkin.Response> instance)
    {
      return TCollections.GetHashCode(instance);
    }


    public static List<global::Jaeger.Thrift.Agent.Zipkin.Response> DeepCopy(this List<global::Jaeger.Thrift.Agent.Zipkin.Response> source)
    {
      if (source == null)
        return null;

      var tmp32 = new List<global::Jaeger.Thrift.Agent.Zipkin.Response>(source.Count);
      foreach (var elem in source)
        tmp32.Add((elem != null) ? elem.DeepCopy() : null);
      return tmp32;
    }


    public static bool Equals(this List<global::Jaeger.Thrift.Agent.Zipkin.Span> instance, object that)
    {
      if (!(that is List<global::Jaeger.Thrift.Agent.Zipkin.Span> other)) return false;
      if (ReferenceEquals(instance, other)) return true;

      return TCollections.Equals(instance, other);
    }


    public static int GetHashCode(this List<global::Jaeger.Thrift.Agent.Zipkin.Span> instance)
    {
      return TCollections.GetHashCode(instance);
    }


    public static List<global::Jaeger.Thrift.Agent.Zipkin.Span> DeepCopy(this List<global::Jaeger.Thrift.Agent.Zipkin.Span> source)
    {
      if (source == null)
        return null;

      var tmp33 = new List<global::Jaeger.Thrift.Agent.Zipkin.Span>(source.Count);
      foreach (var elem in source)
        tmp33.Add((elem != null) ? elem.DeepCopy() : null);
      return tmp33;
    }


  }
}