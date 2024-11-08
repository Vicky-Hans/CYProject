/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

#if XLUA
namespace HybridCLR.NativeBridge
{
	using System;
	using System.Runtime.InteropServices;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || XLUA_GENERAL || (UNITY_WSA && !UNITY_EDITOR)
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate int lua_CSFunction(IntPtr L);

#if GEN_CODE_MINIMIZE
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int CSharpWrapperCaller(IntPtr L, int funcidx, int top);
#endif
#else
	public delegate int lua_CSFunction(IntPtr L);

#if GEN_CODE_MINIMIZE
    public delegate int CSharpWrapperCaller(IntPtr L, int funcidx, int top);
#endif
#endif

	public static class HybridCLRNativeApi
	{
#if (UNITY_IPHONE || UNITY_TVOS || UNITY_WEBGL || UNITY_SWITCH) && !UNITY_EDITOR
        const string LUADLL = "__Internal";
#else
		const string LUADLL = "xlua";
#endif
		
//-----LuaDLL Gen Start

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool xlua_get_hotfix_flag(int idx);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void xlua_set_hotfix_flag(int idx, bool flag);
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr lua_tothread(IntPtr L, int index);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int xlua_get_lib_version();

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int lua_gc(IntPtr L, int what, int data);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr lua_getupvalue(IntPtr L, int funcindex, int n);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr lua_setupvalue(IntPtr L, int funcindex, int n);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int lua_pushthread(IntPtr L);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int lua_setfenv(IntPtr L, int stackPos);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr luaL_newstate();

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_close(IntPtr L);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)] //[-0, +0, m]
		public static extern void luaopen_xlua(IntPtr L);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)] //[-0, +0, m]
		public static extern void luaL_openlibs(IntPtr L);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern uint xlua_objlen(IntPtr L, int stackPos);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_createtable(IntPtr L, int narr, int nrec); //[-0, +0, m]

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int xlua_getglobal(IntPtr L, string name); //[-1, +0, m]

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int xlua_setglobal(IntPtr L, string name); //[-1, +0, m]

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void xlua_getloaders(IntPtr L);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_settop(IntPtr L, int newTop);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_insert(IntPtr L, int newTop);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_remove(IntPtr L, int index);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int lua_rawget(IntPtr L, int index);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_rawset(IntPtr L, int index); //[-2, +0, m]

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int lua_setmetatable(IntPtr L, int objIndex);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int lua_rawequal(IntPtr L, int index1, int index2);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_pushvalue(IntPtr L, int index);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_pushcclosure(IntPtr L, IntPtr fn, int n); //[-n, +1, m]

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_replace(IntPtr L, int index);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int lua_gettop(IntPtr L);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int lua_type(IntPtr L, int index);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool lua_isnumber(IntPtr L, int index);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int luaL_ref(IntPtr L, int registryIndex);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void xlua_rawgeti(IntPtr L, int tableIndex, long index);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void xlua_rawseti(IntPtr L, int tableIndex, long index); //[-1, +0, m]

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int pcall_prepare(IntPtr L, int error_func_ref, int func_ref);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void luaL_unref(IntPtr L, int registryIndex, int reference);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool lua_isstring(IntPtr L, int index);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool lua_isinteger(IntPtr L, int index);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_pushnil(IntPtr L);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int xlua_upvalueindex(int n);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int lua_pcall(IntPtr L, int nArgs, int nResults, int errfunc);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern double lua_tonumber(IntPtr L, int index);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int xlua_tointeger(IntPtr L, int index);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern uint xlua_touint(IntPtr L, int index);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool lua_toboolean(IntPtr L, int index);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr lua_topointer(IntPtr L, int index);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr lua_tolstring(IntPtr L, int index, out IntPtr strLen); //[-0, +0, m]

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr lua_atpanic(IntPtr L, lua_CSFunction panicf);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_pushnumber(IntPtr L, double number);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_pushboolean(IntPtr L, bool value);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void xlua_pushinteger(IntPtr L, int value);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void xlua_pushuint(IntPtr L, uint value);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_pushstring(IntPtr L, string str);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void xlua_pushlstring(IntPtr L, byte[] str, int size);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int luaL_newmetatable(IntPtr L, string meta); //[-0, +1, m]

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int xlua_pgettable(IntPtr L, int idx);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int xlua_psettable(IntPtr L, int idx);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int xluaL_loadbuffer(IntPtr L, byte[] buff, int size, string name);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int xlua_tocsobj_safe(IntPtr L, int obj); //[-0, +0, m]

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int xlua_tocsobj_fast(IntPtr L, int obj);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool lua_checkstack(IntPtr L, int extra); //[-0, +0, m]

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int lua_next(IntPtr L, int index); //[-1, +(2|0), e]

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_pushlightuserdata(IntPtr L, IntPtr udata);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr xlua_tag();

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void luaL_where(IntPtr L, int level); //[-0, +1, m]

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int xlua_tryget_cachedud(IntPtr L, int key, int cache_ref);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void xlua_pushcsobj(IntPtr L, int key, int meta_ref, bool need_cache, int cache_ref); //[-0, +1, m]

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]//[,,m]
		public static extern int gen_obj_indexer(IntPtr L);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]//[,,m]
		public static extern int gen_obj_newindexer(IntPtr L);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]//[,,m]
		public static extern int gen_cls_indexer(IntPtr L);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]//[,,m]
		public static extern int gen_cls_newindexer(IntPtr L);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]//[,,m]
		public static extern int get_error_func_ref(IntPtr L);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]//[,,m]
		public static extern int load_error_func(IntPtr L, int Ref);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int luaopen_i64lib(IntPtr L); //[,,m]

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int luaopen_socket_core(IntPtr L); //[,,m]

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_pushint64(IntPtr L, long n); //[,,m]

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_pushuint64(IntPtr L, ulong n); //[,,m]

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool lua_isint64(IntPtr L, int idx);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool lua_isuint64(IntPtr L, int idx);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern long lua_toint64(IntPtr L, int idx);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern ulong lua_touint64(IntPtr L, int idx);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void xlua_push_csharp_function(IntPtr L, IntPtr fn, int n); //[-0,+1,m]

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int xlua_csharp_str_error(IntPtr L, string message); //[-0,+1,m]

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]//[-0,+0,m]
		public static extern int xlua_csharp_error(IntPtr L);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool xlua_pack_int8_t(IntPtr buff, int offset, byte field);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool xlua_unpack_int8_t(IntPtr buff, int offset, out byte field);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool xlua_pack_int16_t(IntPtr buff, int offset, short field);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool xlua_unpack_int16_t(IntPtr buff, int offset, out short field);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool xlua_pack_int32_t(IntPtr buff, int offset, int field);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool xlua_unpack_int32_t(IntPtr buff, int offset, out int field);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool xlua_pack_int64_t(IntPtr buff, int offset, long field);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool xlua_unpack_int64_t(IntPtr buff, int offset, out long field);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool xlua_pack_float(IntPtr buff, int offset, float field);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool xlua_unpack_float(IntPtr buff, int offset, out float field);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool xlua_pack_double(IntPtr buff, int offset, double field);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool xlua_unpack_double(IntPtr buff, int offset, out double field);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr xlua_pushstruct(IntPtr L, uint size, int meta_ref);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void xlua_pushcstable(IntPtr L, uint field_count, int meta_ref);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr lua_touserdata(IntPtr L, int idx);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int xlua_gettypeid(IntPtr L, int idx);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int xlua_get_registry_index();

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int xlua_pgettable_bypath(IntPtr L, int idx, string path);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int xlua_psettable_bypath(IntPtr L, int idx, string path);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool xlua_pack_float2(IntPtr buff, int offset, float f1, float f2);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool xlua_unpack_float2(IntPtr buff, int offset, out float f1, out float f2);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool xlua_pack_float3(IntPtr buff, int offset, float f1, float f2, float f3);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool xlua_unpack_float3(IntPtr buff, int offset, out float f1, out float f2, out float f3);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool xlua_pack_float4(IntPtr buff, int offset, float f1, float f2, float f3, float f4);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool xlua_unpack_float4(IntPtr buff, int offset, out float f1, out float f2, out float f3, out float f4);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool xlua_pack_float5(IntPtr buff, int offset, float f1, float f2, float f3, float f4, float f5);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool xlua_unpack_float5(IntPtr buff, int offset, out float f1, out float f2, out float f3, out float f4, out float f5);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool xlua_pack_float6(IntPtr buff, int offset, float f1, float f2, float f3, float f4, float f5, float f6);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool xlua_unpack_float6(IntPtr buff, int offset, out float f1, out float f2, out float f3, out float f4, out float f5, out float f6);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool xlua_pack_decimal(IntPtr buff, int offset, ref decimal dec);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool xlua_unpack_decimal(IntPtr buff, int offset, out byte scale, out byte sign, out int hi32, out ulong lo64);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool xlua_is_eq_str(IntPtr L, int index, string str, int str_len);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr xlua_gl(IntPtr L);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void xlua_set_csharp_wrapper_caller(IntPtr wrapper);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void xlua_push_csharp_wrapper(IntPtr L, int wrapperID);

		//rapidjson
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int luaopen_rapidjson(System.IntPtr L);

		// lua-profobuf
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int luaopen_pb(IntPtr L);

		//-----LuaDLL Gen End
//-----Gen End
	}
}
#endif