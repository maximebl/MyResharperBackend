package model.rider

import com.jetbrains.rd.generator.nova.*
import com.jetbrains.rd.generator.nova.Ext
import com.jetbrains.rd.generator.nova.PredefinedType
import com.jetbrains.rd.generator.nova.array
import com.jetbrains.rd.generator.nova.call
import com.jetbrains.rd.generator.nova.csharp.CSharp50Generator
import com.jetbrains.rd.generator.nova.kotlin.Kotlin11Generator
import com.jetbrains.rd.generator.nova.property
import com.jetbrains.rd.generator.nova.setting
import com.jetbrains.rd.generator.nova.signal
import com.jetbrains.rider.model.nova.ide.SolutionModel

@Suppress("unused")
object MyBackendModel : Ext(SolutionModel.Solution) {
    init {
        setting(CSharp50Generator.Namespace, "JetBrains.Rider.Model")
        setting(Kotlin11Generator.Namespace, "com.jetbrains.rider.model")
    }

    // Bidirectional event
    val mycoovalue = signal("mycoolvalue", PredefinedType.string)

    val getFunctionNames = call(
        "getFunctionNames",
        paramType = PredefinedType.string,
        resultType = array(PredefinedType.string)
    );
}
