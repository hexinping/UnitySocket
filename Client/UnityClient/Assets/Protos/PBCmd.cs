// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: PBCmd.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace PB {

  /// <summary>Holder for reflection information generated from PBCmd.proto</summary>
  public static partial class PBCmdReflection {

    #region Descriptor
    /// <summary>File descriptor for PBCmd.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static PBCmdReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CgtQQkNtZC5wcm90bxICUEIqXgoIRW5tQ21kSUQSDAoIQ1NfTE9HSU4QABIM",
            "CghTQ19MT0dJThABEgsKB0NTX1BJTkcQAhILCgdTQ19QSU5HEAMSDQoJQ1Nf",
            "UEVSU09OEGUSDQoJU0NfUEVSU09OEGZiBnByb3RvMw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(new[] {typeof(global::PB.EnmCmdID), }, null, null));
    }
    #endregion

  }
  #region Enums
  public enum EnmCmdID {
    /// <summary>
    ///登录请求协议号
    /// </summary>
    [pbr::OriginalName("CS_LOGIN")] CsLogin = 0,
    /// <summary>
    ///登录请求回包协议号
    /// </summary>
    [pbr::OriginalName("SC_LOGIN")] ScLogin = 1,
    /// <summary>
    ///客户端发送心跳
    /// </summary>
    [pbr::OriginalName("CS_PING")] CsPing = 2,
    /// <summary>
    ///服务器接收心跳
    /// </summary>
    [pbr::OriginalName("SC_PING")] ScPing = 3,
    /// <summary>
    ///客户端发送心跳
    /// </summary>
    [pbr::OriginalName("CS_PERSON")] CsPerson = 101,
    /// <summary>
    ///服务器接收心跳
    /// </summary>
    [pbr::OriginalName("SC_PERSON")] ScPerson = 102,
  }

  #endregion

}

#endregion Designer generated code
