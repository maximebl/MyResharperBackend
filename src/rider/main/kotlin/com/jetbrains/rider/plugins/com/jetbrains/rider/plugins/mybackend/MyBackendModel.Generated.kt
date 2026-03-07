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
    private val _getFunctionNames: RdCall<MyFindRequest, WalkedResult>,
    private val _getUsages: RdCall<MyFindRequest, WalkedResult>,
    private val _onUsagesStarted: RdSignal<Int>,
    private val _onUsageFound: RdSignal<WalkedFunction>
) : RdExtBase() {
    //companion

    companion object : ISerializersOwner {

        override fun registerSerializersCore(serializers: ISerializers)  {
            val classLoader = javaClass.classLoader
            serializers.register(LazyCompanionMarshaller(RdId(-7237491644472025097), classLoader, "com.jetbrains.rider.model.MyFindRequest"))
            serializers.register(LazyCompanionMarshaller(RdId(-2616143937914611350), classLoader, "com.jetbrains.rider.model.StatementInfo"))
            serializers.register(LazyCompanionMarshaller(RdId(1876238290876968723), classLoader, "com.jetbrains.rider.model.WalkedFunction"))
            serializers.register(LazyCompanionMarshaller(RdId(-1188160139399588328), classLoader, "com.jetbrains.rider.model.WalkedResult"))
        }





        const val serializationHash = -7067078345171135611L

    }
    override val serializersOwner: ISerializersOwner get() = MyBackendModel
    override val serializationHash: Long get() = MyBackendModel.serializationHash

    //fields
    val getFunctionNames: IRdCall<MyFindRequest, WalkedResult> get() = _getFunctionNames
    val getUsages: IRdCall<MyFindRequest, WalkedResult> get() = _getUsages
    val onUsagesStarted: ISignal<Int> get() = _onUsagesStarted
    val onUsageFound: ISignal<WalkedFunction> get() = _onUsageFound
    //methods
    //initializer
    init {
        bindableChildren.add("getFunctionNames" to _getFunctionNames)
        bindableChildren.add("getUsages" to _getUsages)
        bindableChildren.add("onUsagesStarted" to _onUsagesStarted)
        bindableChildren.add("onUsageFound" to _onUsageFound)
    }

    //secondary constructor
    internal constructor(
    ) : this(
        RdCall<MyFindRequest, WalkedResult>(MyFindRequest, WalkedResult),
        RdCall<MyFindRequest, WalkedResult>(MyFindRequest, WalkedResult),
        RdSignal<Int>(FrameworkMarshallers.Int),
        RdSignal<WalkedFunction>(WalkedFunction)
    )

    //equals trait
    //hash code trait
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("MyBackendModel (")
        printer.indent {
            print("getFunctionNames = "); _getFunctionNames.print(printer); println()
            print("getUsages = "); _getUsages.print(printer); println()
            print("onUsagesStarted = "); _onUsagesStarted.print(printer); println()
            print("onUsageFound = "); _onUsageFound.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    override fun deepClone(): MyBackendModel   {
        return MyBackendModel(
            _getFunctionNames.deepClonePolymorphic(),
            _getUsages.deepClonePolymorphic(),
            _onUsagesStarted.deepClonePolymorphic(),
            _onUsageFound.deepClonePolymorphic()
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


/**
 * #### Generated from [MyBackendModel.kt:34]
 */
data class WalkedFunction (
    val name: String,
    val signature: String,
    val path: String,
    val offset: Int,
    val statements: List<StatementInfo>
) : IPrintable {
    //companion

    companion object : IMarshaller<WalkedFunction> {
        override val _type: KClass<WalkedFunction> = WalkedFunction::class
        override val id: RdId get() = RdId(1876238290876968723)

        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): WalkedFunction  {
            val name = buffer.readString()
            val signature = buffer.readString()
            val path = buffer.readString()
            val offset = buffer.readInt()
            val statements = buffer.readList { StatementInfo.read(ctx, buffer) }
            return WalkedFunction(name, signature, path, offset, statements)
        }

        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: WalkedFunction)  {
            buffer.writeString(value.name)
            buffer.writeString(value.signature)
            buffer.writeString(value.path)
            buffer.writeInt(value.offset)
            buffer.writeList(value.statements) { v -> StatementInfo.write(ctx, buffer, v) }
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
        
        other as WalkedFunction
        
        if (name != other.name) return false
        if (path != other.path) return false
        if (offset != other.offset) return false
        if (statements != other.statements) return false
        
        return true
    }
    //hash code trait
    override fun hashCode(): Int  {
        var __r = 0
        __r = __r*31 + name.hashCode()
        __r = __r*31 + path.hashCode()
        __r = __r*31 + offset.hashCode()
        __r = __r*31 + statements.hashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("WalkedFunction (")
        printer.indent {
            print("name = "); name.print(printer); println()
            print("path = "); path.print(printer); println()
            print("offset = "); offset.print(printer); println()
            print("statements = "); statements.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    //contexts
    //threading
}


/**
 * #### Generated from [MyBackendModel.kt:41]
 */
data class WalkedResult (
    val current: WalkedFunction,
    val usages: List<WalkedFunction>
) : IPrintable {
    //companion
    
    companion object : IMarshaller<WalkedResult> {
        override val _type: KClass<WalkedResult> = WalkedResult::class
        override val id: RdId get() = RdId(-1188160139399588328)
        
        @Suppress("UNCHECKED_CAST")
        override fun read(ctx: SerializationCtx, buffer: AbstractBuffer): WalkedResult  {
            val current = WalkedFunction.read(ctx, buffer)
            val usages = buffer.readList { WalkedFunction.read(ctx, buffer) }
            return WalkedResult(current, usages)
        }
        
        override fun write(ctx: SerializationCtx, buffer: AbstractBuffer, value: WalkedResult)  {
            WalkedFunction.write(ctx, buffer, value.current)
            buffer.writeList(value.usages) { v -> WalkedFunction.write(ctx, buffer, v) }
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
        
        other as WalkedResult
        
        if (current != other.current) return false
        if (usages != other.usages) return false
        
        return true
    }
    //hash code trait
    override fun hashCode(): Int  {
        var __r = 0
        __r = __r*31 + current.hashCode()
        __r = __r*31 + usages.hashCode()
        return __r
    }
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("WalkedResult (")
        printer.indent {
            print("current = "); current.print(printer); println()
            print("usages = "); usages.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    //contexts
    //threading
}
