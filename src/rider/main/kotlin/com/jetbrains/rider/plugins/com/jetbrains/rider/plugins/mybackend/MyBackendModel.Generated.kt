@file:Suppress("EXPERIMENTAL_API_USAGE","EXPERIMENTAL_UNSIGNED_LITERALS","PackageDirectoryMismatch","UnusedImport","unused","LocalVariableName","CanBeVal","PropertyName","EnumEntryName","ClassName","ObjectPropertyName","UnnecessaryVariable","SpellCheckingInspection")
package com.jetbrains.rider.model

import com.jetbrains.rd.framework.*
import com.jetbrains.rd.framework.base.*
import com.jetbrains.rd.framework.impl.*

import com.jetbrains.rd.util.lifetime.*
import com.jetbrains.rd.util.reactive.*
import com.jetbrains.rd.util.string.*
import com.jetbrains.rd.util.*
import kotlin.time.Duration
import kotlin.reflect.KClass
import kotlin.jvm.JvmStatic



/**
 * #### Generated from [MyBackendModel.kt:16]
 */
class MyBackendModel private constructor(
    private val _getFunctionNames: RdCall<MyFindRequest, Array<StatementInfo>>
) : RdExtBase() {
    //companion
    
    companion object : ISerializersOwner {
        
        override fun registerSerializersCore(serializers: ISerializers)  {
            val classLoader = javaClass.classLoader
            serializers.register(LazyCompanionMarshaller(RdId(-7237491644472025097), classLoader, "com.jetbrains.rider.model.MyFindRequest"))
            serializers.register(LazyCompanionMarshaller(RdId(-2616143937914611350), classLoader, "com.jetbrains.rider.model.StatementInfo"))
        }
        
        
        
        
        private val __StatementInfoArraySerializer = StatementInfo.array()
        
        const val serializationHash = 4223364129162007567L
        
    }
    override val serializersOwner: ISerializersOwner get() = MyBackendModel
    override val serializationHash: Long get() = MyBackendModel.serializationHash
    
    //fields
    val getFunctionNames: IRdCall<MyFindRequest, Array<StatementInfo>> get() = _getFunctionNames
    //methods
    //initializer
    init {
        bindableChildren.add("getFunctionNames" to _getFunctionNames)
    }
    
    //secondary constructor
    internal constructor(
    ) : this(
        RdCall<MyFindRequest, Array<StatementInfo>>(MyFindRequest, __StatementInfoArraySerializer)
    )
    
    //equals trait
    //hash code trait
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("MyBackendModel (")
        printer.indent {
            print("getFunctionNames = "); _getFunctionNames.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    override fun deepClone(): MyBackendModel   {
        return MyBackendModel(
            _getFunctionNames.deepClonePolymorphic()
        )
    }
    //contexts
    //threading
    override val extThreading: ExtThreadingKind get() = ExtThreadingKind.Default
}
val com.jetbrains.rd.ide.model.Solution.myBackendModel get() = getOrCreateExtension("myBackendModel", ::MyBackendModel)



/**
 * #### Generated from [MyBackendModel.kt:23]
 */
data class MyFindRequest (
    val filePath: String,
    val caretOffset: Int
) : IPrintable {
    //companion
    
    companion object : IMarshaller<MyFindRequest> {
        override val _type: KClass<MyFindRequest> = MyFindRequest::class
        override val id: RdId get() = RdId(-7237491644472025097)
        
        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): MyFindRequest  {
            val filePath = buffer.readString()
            val caretOffset = buffer.readInt()
            return MyFindRequest(filePath, caretOffset)
        }
        
        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: MyFindRequest)  {
            buffer.writeString(value.filePath)
            buffer.writeInt(value.caretOffset)
        }
        
        
    }
    //fields
    //methods
    //initializer
    //secondary constructor
    //equals trait
    override fun equals(other: Any?): Boolean  {
        if (this === other) return true
        if (other == null || other::class != this::class) return false
        
        other as MyFindRequest
        
        if (filePath != other.filePath) return false
        if (caretOffset != other.caretOffset) return false
        
        return true
    }
    //hash code trait
    override fun hashCode(): Int  {
        var __r = 0
        __r = __r*31 + filePath.hashCode()
        __r = __r*31 + caretOffset.hashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("MyFindRequest (")
        printer.indent {
            print("filePath = "); filePath.print(printer); println()
            print("caretOffset = "); caretOffset.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    //contexts
    //threading
}


/**
 * #### Generated from [MyBackendModel.kt:29]
 */
data class StatementInfo (
    val name: String,
    val offset: Int
) : IPrintable {
    //companion
    
    companion object : IMarshaller<StatementInfo> {
        override val _type: KClass<StatementInfo> = StatementInfo::class
        override val id: RdId get() = RdId(-2616143937914611350)
        
        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): StatementInfo  {
            val name = buffer.readString()
            val offset = buffer.readInt()
            return StatementInfo(name, offset)
        }
        
        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: StatementInfo)  {
            buffer.writeString(value.name)
            buffer.writeInt(value.offset)
        }
        
        
    }
    //fields
    //methods
    //initializer
    //secondary constructor
    //equals trait
    override fun equals(other: Any?): Boolean  {
        if (this === other) return true
        if (other == null || other::class != this::class) return false
        
        other as StatementInfo
        
        if (name != other.name) return false
        if (offset != other.offset) return false
        
        return true
    }
    //hash code trait
    override fun hashCode(): Int  {
        var __r = 0
        __r = __r*31 + name.hashCode()
        __r = __r*31 + offset.hashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("StatementInfo (")
        printer.indent {
            print("name = "); name.print(printer); println()
            print("offset = "); offset.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    //contexts
    //threading
}
