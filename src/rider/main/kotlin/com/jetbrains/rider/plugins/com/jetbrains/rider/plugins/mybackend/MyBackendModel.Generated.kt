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
    private val _mycoolvalue: RdSignal<String>,
    private val _getFunctionNames: RdCall<String, Array<String>>
) : RdExtBase() {
    //companion
    
    companion object : ISerializersOwner {
        
        override fun registerSerializersCore(serializers: ISerializers)  {
        }
        
        
        
        
        private val __StringArraySerializer = FrameworkMarshallers.String.array()
        
        const val serializationHash = -8484046031559129444L
        
    }
    override val serializersOwner: ISerializersOwner get() = MyBackendModel
    override val serializationHash: Long get() = MyBackendModel.serializationHash
    
    //fields
    val mycoolvalue: ISignal<String> get() = _mycoolvalue
    val getFunctionNames: IRdCall<String, Array<String>> get() = _getFunctionNames
    //methods
    //initializer
    init {
        bindableChildren.add("mycoolvalue" to _mycoolvalue)
        bindableChildren.add("getFunctionNames" to _getFunctionNames)
    }
    
    //secondary constructor
    internal constructor(
    ) : this(
        RdSignal<String>(FrameworkMarshallers.String),
        RdCall<String, Array<String>>(FrameworkMarshallers.String, __StringArraySerializer)
    )
    
    //equals trait
    //hash code trait
    //pretty print
    override fun print(printer: PrettyPrinter)  {
        printer.println("MyBackendModel (")
        printer.indent {
            print("mycoolvalue = "); _mycoolvalue.print(printer); println()
            print("getFunctionNames = "); _getFunctionNames.print(printer); println()
        }
        printer.print(")")
    }
    //deepClone
    override fun deepClone(): MyBackendModel   {
        return MyBackendModel(
            _mycoolvalue.deepClonePolymorphic(),
            _getFunctionNames.deepClonePolymorphic()
        )
    }
    //contexts
    //threading
    override val extThreading: ExtThreadingKind get() = ExtThreadingKind.Default
}
val com.jetbrains.rd.ide.model.Solution.myBackendModel get() = getOrCreateExtension("myBackendModel", ::MyBackendModel)

