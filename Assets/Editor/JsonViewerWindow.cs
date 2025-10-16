using UnityEditor;                  // Permite crear ventanas y utilidades que solo corren dentro del Editor de Unity.
using UnityEngine;                  // Namespace base de Unity (GameObjects, Transform, MonoBehaviour, etc.).
using System.IO;                    // Para manejar archivos y rutas del sistema (leer/escribir archivos).
using System.Collections.Generic;   // Para usar listas y diccionarios genéricos.
using Newtonsoft.Json.Linq;         // Biblioteca JSON (JObject, JArray...) para parsear y manipular JSON dinámicamente.

// Definimos una ventana del editor llamada JsonManagerWindow que hereda de EditorWindow.
public class JsonManagerWindow : EditorWindow
{
    // Enum que describe los modos en los que puede estar la ventana (crear modelo, crear json, ver json).
    private enum JsonMode { CrearModelo, CrearJsonDesdeModelo, VerJson }

    // Variable que almacena el modo actual (por defecto al crear la ventana será CrearModelo).
    private JsonMode currentMode = JsonMode.CrearModelo;

    // ----------------------------------------------------
    // Variables comunes / estado de la ventana
    // ----------------------------------------------------
    private Vector2 scrollPos; // Posición de scroll para las áreas con contenido desplazable.
    private string basePath => Path.Combine(Application.dataPath, "JsonModels");    // Carpeta base donde se guardan los modelos (.jsonmodel)

    // ----------------------------------------------------
    // --- Campos para "Crear modelo" ---
    // ----------------------------------------------------
    private string modelName = ""; // Nombre del modelo que vamos a crear o editar.
    private List<KeyValuePair<string, string>> modelFields = new List<KeyValuePair<string, string>>();
    // Lista de pares clave:valor que representa los campos del modelo y su valor por defecto.

    // ----------------------------------------------------
    // --- Campos para "Crear JSON desde modelo" ---
    // ----------------------------------------------------
    private string currentJsonModelName = null;                                     // Nombre del modelo del JSON que estamos editando (si corresponde).
    private string[] availableModels;                                               // Lista de modelos disponibles en la carpeta base.
    private int selectedModelIndex = 0;                                             // Índice del modelo seleccionado en el popup.
    private JObject modelTemplate;                                                  // Template (JObject) cargado del archivo .jsonmodel
    private Dictionary<string, string> jsonValues = new Dictionary<string, string>();
    // Diccionario con los valores que el usuario ingresa para crear el JSON desde la plantilla.

    // ----------------------------------------------------
    // --- Campos para "Ver / Editar JSON" ---
    // ----------------------------------------------------
    private string jsonFilePath = "";                                               // Ruta completa del JSON que se está viendo/ editando.
    private string jsonText = "";                                                   // Texto crudo del JSON (para edición directa).
    private JObject jsonObject;                                                     // Objeto parseado (JObject) del JSON cargado.
    private bool isEditing = false;                                                 // Flag que indica si estamos en modo edición (texto editable) o solo visualización.

    // Agrega la opción en el menú "Tools" para abrir esta ventana del editor.
    [MenuItem("Tools/JSON Manager")]
    public static void ShowWindow()
    {
        // Obtiene (o crea) la ventana y le pone título "JSON Manager".
        GetWindow<JsonManagerWindow>("JSON Manager");
    }

    // Este método se llama cuando la ventana se habilita / abre.
    private void OnEnable()
    {
        // Si la carpeta base (Assets/JsonModels) no existe, la crea.
        if (!Directory.Exists(basePath))
            Directory.CreateDirectory(basePath);

        // Carga los modelos disponibles en la carpeta base.
        LoadAvailableModels();
    }

    // Método principal de GUI que Unity llama varias veces por segundo para dibujar la ventana.
    private void OnGUI()
    {
        // Título grande de la ventana.
        GUILayout.Label("🧩 JSON Manager", EditorStyles.boldLabel);
        GUILayout.Space(5); // Espacio pequeño.

        // Barra horizontal con toggles para cambiar entre modos.
        GUILayout.BeginHorizontal();
        // Si el toggle "Crear Modelo" está activo, se establece el modo correspondiente.
        if (GUILayout.Toggle(currentMode == JsonMode.CrearModelo, "Crear Modelo", EditorStyles.toolbarButton))
            currentMode = JsonMode.CrearModelo;
        // Toggle para "Crear JSON desde Modelo".
        if (GUILayout.Toggle(currentMode == JsonMode.CrearJsonDesdeModelo, "Crear JSON desde Modelo", EditorStyles.toolbarButton))
            currentMode = JsonMode.CrearJsonDesdeModelo;
        // Toggle para "Ver / Editar JSON".
        if (GUILayout.Toggle(currentMode == JsonMode.VerJson, "Leer / Editar JSON", EditorStyles.toolbarButton))
            currentMode = JsonMode.VerJson;
        GUILayout.EndHorizontal();

        GUILayout.Space(10); // Separador visual.

        // Según el modo actual, dibuja la UI correspondiente.
        switch (currentMode)
        {
            case JsonMode.CrearModelo:
                DrawCreateModel();
                break;
            case JsonMode.CrearJsonDesdeModelo:
                DrawCreateFromModel();
                break;
            case JsonMode.VerJson:
                DrawJsonViewer();
                break;
        }
    }

    // ================================================================
    // 🧩 MODO 1: Crear Modelo - Dibuja la interfaz para crear modelos
    // ================================================================
    private void DrawCreateModel()
    {
        GUILayout.Label("Crear o editar modelo JSON", EditorStyles.boldLabel);

        // Sección para editar/crear un nuevo modelo
        GUILayout.Space(5);
        modelName = EditorGUILayout.TextField("Nombre del modelo:", modelName);     // Campo para el nombre del modelo

        GUILayout.Space(5);
        GUILayout.Label("Campos del modelo (nombre + valor por defecto):", EditorStyles.miniBoldLabel);

        // Área scroll para los campos del modelo (limitada en altura).
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
        for (int i = 0; i < modelFields.Count; i++)
        {
            // Cada línea muestra dos textfields (clave y valor) y un botón para eliminar el campo.
            EditorGUILayout.BeginHorizontal();
            modelFields[i] = new KeyValuePair<string, string>(
                EditorGUILayout.TextField(modelFields[i].Key),                      // nombre del campo
                EditorGUILayout.TextField(modelFields[i].Value)                     // valor por defecto
            );
            if (GUILayout.Button("X", GUILayout.Width(20)))
                modelFields.RemoveAt(i);                                            // Si se presiona X, se elimina el campo actual.
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        // Si existen modelos guardados, los muestra en una lista con opciones para Editar o Eliminar.
        if (availableModels != null && availableModels.Length > 0)
        {
            GUILayout.Label("Modelos existentes:", EditorStyles.miniBoldLabel);
            for (int i = 0; i < availableModels.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(availableModels[i]);                                // Muestra el nombre del modelo.
                if (GUILayout.Button("Editar", GUILayout.Width(80)))
                {
                    LoadModelForEditing(availableModels[i]);                        // Cargar el modelo para editarlo.
                }
                if (GUILayout.Button("🗑️", GUILayout.Width(30)))
                {
                    // Confirmación antes de borrar el archivo del modelo.
                    if (EditorUtility.DisplayDialog("Eliminar modelo", $"¿Seguro que deseas eliminar '{availableModels[i]}'?", "Sí", "No"))
                    {
                        string modelPath = Path.Combine(basePath, availableModels[i] + ".jsonmodel");
                        File.Delete(modelPath);                                     // Borra el archivo físico.
                        AssetDatabase.Refresh();                                    // Refresca la base de assets para que Unity lo detecte.
                        LoadAvailableModels();                                      // Recarga la lista de modelos disponibles.
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.Space(10);
        }

        // Botón para añadir un nuevo campo vacío.
        if (GUILayout.Button("Agregar Campo"))
            modelFields.Add(new KeyValuePair<string, string>("", ""));

        GUILayout.Space(10);

        // Botón para guardar el modelo actual en disco (.jsonmodel)
        if (GUILayout.Button("💾 Guardar Modelo"))
            SaveModel();
    }

    // Carga un modelo desde disco para editarlo en la ventana.
    private void LoadModelForEditing(string modelToLoad)
    {
        try
        {
            string path = Path.Combine(basePath, modelToLoad + ".jsonmodel");       // Ruta del archivo
            string content = File.ReadAllText(path);                                // Lee todo el contenido
            JObject obj = JObject.Parse(content);                                   // Parsea el JSON a JObject

            modelName = modelToLoad;                                                // Asigna el nombre del modelo al campo
            modelFields.Clear();                                                    // Limpia la lista actual de campos

            // Recorre las propiedades del JObject y las añade como pares clave-valor
            foreach (var prop in obj)
                modelFields.Add(new KeyValuePair<string, string>(prop.Key, prop.Value.ToString()));
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al cargar modelo: {e.Message}");                 // Log en consola en caso de error.
        }
    }

    // Guarda el modelo actual en un archivo .jsonmodel dentro de la carpeta base.
    private void SaveModel()
    {
        // Validación: necesita nombre del modelo.
        if (string.IsNullOrEmpty(modelName))
        {
            EditorUtility.DisplayDialog("Error", "Debes ingresar un nombre para el modelo.", "OK");
            return;
        }

        // Construye un JObject con los campos especificados.
        JObject obj = new JObject();
        foreach (var field in modelFields)
            obj[field.Key] = field.Value;

        // Ruta final para guardar el archivo del modelo.
        string path = Path.Combine(basePath, modelName + ".jsonmodel");
        File.WriteAllText(path, obj.ToString());                                    // Escribe el JSON en disco.
        AssetDatabase.Refresh();                                                    // Refresca assets para que Unity detecte el nuevo archivo.

        LoadAvailableModels();                                                      // Recarga la lista de modelos disponibles.

        EditorUtility.DisplayDialog("Éxito", $"Modelo '{modelName}' guardado correctamente.", "OK"); // Mensaje de éxito.
    }

    // ============================================================
    // 🏗️ MODO 2: Crear JSON desde Modelo - Interfaz para crear JSON
    // ============================================================
    private void DrawCreateFromModel()
    {
        GUILayout.Label("Crear JSON a partir de un modelo", EditorStyles.boldLabel);

        // Si no hay modelos, muestra información y sale.
        if (availableModels == null || availableModels.Length == 0)
        {
            EditorGUILayout.HelpBox("No hay modelos guardados. Crea uno primero.", MessageType.Info);
            return;
        }

        // Popup para seleccionar el modelo de la lista.
        selectedModelIndex = EditorGUILayout.Popup("Modelo:", selectedModelIndex, availableModels);

        // Botón para cargar la plantilla del modelo seleccionado.
        if (GUILayout.Button("Cargar Modelo"))
            LoadModelTemplate(availableModels[selectedModelIndex]);

        // Si la plantilla está cargada, muestra los campos para completarlos.
        if (modelTemplate != null)
        {
            GUILayout.Space(10);
            GUILayout.Label("Completa los valores:", EditorStyles.miniBoldLabel);

            // Área scroll con todos los campos de la plantilla.
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(300));
            foreach (var prop in modelTemplate)
            {
                // Si no existe la clave en jsonValues, la inicializa con el valor del template.
                if (!jsonValues.ContainsKey(prop.Key))
                    jsonValues[prop.Key] = prop.Value.ToString();

                // Muestra un TextField para editar el valor asociado a cada clave.
                jsonValues[prop.Key] = EditorGUILayout.TextField(prop.Key, jsonValues[prop.Key]);
            }
            EditorGUILayout.EndScrollView();

            GUILayout.Space(10);
            // Botón para guardar el JSON nuevo usando los valores ingresados.
            if (GUILayout.Button("💾 Guardar JSON"))
                SaveJsonFromModel();

            // Si hay una ruta de JSON cargada (estamos editando un JSON existente), permite guardar cambios.
            if (!string.IsNullOrEmpty(jsonFilePath))
            {
                GUILayout.Space(10);
                if (GUILayout.Button("💾 Guardar Cambios en JSON Existente"))
                    SaveExistingJson();
            }

            GUILayout.Space(10);
            GUILayout.Label("Editar JSON existente basado en modelo", EditorStyles.boldLabel);

            // Botón para abrir un JSON ya existente desde el explorador de archivos.
            if (GUILayout.Button("📂 Cargar JSON existente"))
            {
                string path = EditorUtility.OpenFilePanel("Selecciona un JSON existente", Application.dataPath, "json");
                if (!string.IsNullOrEmpty(path))
                    LoadJsonForModelEditing(path);  // Carga el JSON y lo prepara para edición si corresponde.
            }

        }
    }

    // Guarda los cambios en el JSON que se está editando (jsonFilePath) sobrescribiendo el archivo.
    private void SaveExistingJson()
    {
        // Validación rápida
        if (string.IsNullOrEmpty(jsonFilePath) || string.IsNullOrEmpty(currentJsonModelName))
            return;

        // Construye un JObject con los valores actuales.
        JObject updated = new JObject();

        foreach (var pair in jsonValues)
            updated[pair.Key] = pair.Value;

        // Asegura que el JSON tenga el campo que indica de qué modelo proviene.
        updated["__modelName"] = currentJsonModelName;

        File.WriteAllText(jsonFilePath, updated.ToString());                        // Escribe en disco.
        AssetDatabase.Refresh();                                                    // Refresca assets.

        EditorUtility.DisplayDialog("Éxito", "Cambios guardados correctamente en el JSON.", "OK"); // Mensaje de éxito.
    }

    // Carga un JSON desde disco para editarlo con su modelo asociado (si tiene __modelName).
    private void LoadJsonForModelEditing(string path)
    {
        try
        {
            string content = File.ReadAllText(path);                                // Lee archivo
            JObject loaded = JObject.Parse(content);                                // Parsea JSON

            // Verifica que el JSON tenga información del modelo (campo __modelName).
            if (!loaded.ContainsKey("__modelName"))
            {
                EditorUtility.DisplayDialog("Error", "Este JSON no tiene información del modelo base.", "OK");
                return;
            }

            currentJsonModelName = loaded["__modelName"].ToString();                // Guarda el nombre del modelo.
            LoadModelTemplate(currentJsonModelName);                                // Carga la plantilla del modelo.

            // Llena los valores actuales en jsonValues usando lo cargado y el template como fallback.
            jsonValues.Clear();
            foreach (var prop in modelTemplate)
            {
                if (loaded.ContainsKey(prop.Key))
                    jsonValues[prop.Key] = loaded[prop.Key].ToString();
                else
                    jsonValues[prop.Key] = prop.Value.ToString();
            }

            // Guarda la ruta del JSON que estamos editando.
            jsonFilePath = path;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error al cargar JSON para edición: " + e.Message);
        }
    }

    // Carga la lista de modelos disponibles en la carpeta base (.jsonmodel).
    private void LoadAvailableModels()
    {
        var files = Directory.GetFiles(basePath, "*.jsonmodel");                    // Obtiene todos los .jsonmodel
        availableModels = new string[files.Length];                                 // Inicializa array con la cantidad de archivos
        for (int i = 0; i < files.Length; i++)
            availableModels[i] = Path.GetFileNameWithoutExtension(files[i]);        // Guarda solo el nombre sin extensión
    }

    // Carga la plantilla (JObject) de un modelo específico.
    private void LoadModelTemplate(string modelName)
    {
        string path = Path.Combine(basePath, modelName + ".jsonmodel");             // Ruta del archivo
        string content = File.ReadAllText(path);                                    // Lee contenido
        modelTemplate = JObject.Parse(content);                                     // Parsea y guarda el template
        jsonValues.Clear();                                                         // Limpia cualquier valor previo
    }

    // Crea un nuevo archivo JSON en disco usando los valores ingresados en la interfaz.
    private void SaveJsonFromModel()
    {
        JObject newJson = new JObject(); // Nuevo JObject para el JSON final

        // Guarda todos los pares clave:valor ingresados.
        foreach (var pair in jsonValues)
            newJson[pair.Key] = pair.Value;

        // Agrega un campo oculto para identificar el modelo base del JSON.
        newJson["__modelName"] = availableModels[selectedModelIndex];

        // Abre un dialog para que el usuario elija dónde guardar el archivo JSON.
        string savePath = EditorUtility.SaveFilePanel("Guardar JSON", Application.dataPath, "NuevoArchivo", "json");
        if (!string.IsNullOrEmpty(savePath))
        {
            File.WriteAllText(savePath, newJson.ToString());                        // Escribe archivo
            AssetDatabase.Refresh();                                                // Refresca assets
            EditorUtility.DisplayDialog("Éxito", "Archivo JSON creado correctamente.", "OK");
        }
    }

    // ============================================================
    // 👀 MODO 3: Ver / Editar JSON - Dibuja la interfaz de viewer/editor
    // ============================================================
    private void DrawJsonViewer()
    {
        GUILayout.Label("Ver / Editar JSON", EditorStyles.boldLabel);

        // Botón para seleccionar un JSON desde disco.
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Seleccionar JSON", GUILayout.Width(150)))
        {
            jsonFilePath = EditorUtility.OpenFilePanel("Selecciona un archivo JSON", Application.dataPath, "json");
            if (!string.IsNullOrEmpty(jsonFilePath))
                LoadJson(jsonFilePath);                                             // Carga el JSON seleccionado
        }

        // Muestra el nombre del archivo cargado si hay uno.
        if (!string.IsNullOrEmpty(jsonFilePath))
            GUILayout.Label(Path.GetFileName(jsonFilePath));
        GUILayout.EndHorizontal();

        // Si hay un objeto JSON cargado, muestra opciones de edición/visualización.
        if (jsonObject != null)
        {
            // Botón que alterna entre editar (texto) y guardar.
            if (GUILayout.Button(isEditing ? "💾 Guardar y Volver" : "✏️ Editar JSON", GUILayout.Height(25)))
            {
                if (isEditing)
                {
                    SaveJson();                                                     // Guarda el texto crudo si está en modo edición
                    LoadJson(jsonFilePath);                                         // Recarga para actualizar visualización
                    isEditing = false;                                              // Sale del modo edición
                }
                else
                    isEditing = true;                                               // Entra en modo edición
            }

            GUILayout.Space(10);

            // Área scroll para mostrar el JSON (texto editable o vista estructurada).
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            if (isEditing)
                jsonText = EditorGUILayout.TextArea(jsonText, GUILayout.ExpandHeight(true)); // Editor de texto plano
            else
                DrawJsonObject(jsonObject, 0); // Vista estructurada recursiva
            EditorGUILayout.EndScrollView();
        }

        // Al final, muestra una lista de todos los JSON dentro del proyecto para abrir rápidamente.
        DrawJsonListSection();
    }

    // Muestra la lista completa de archivos .json dentro del proyecto (Assets).
    void DrawJsonListSection()
    {
        GUILayout.Label("📜 Lista completa de JSON en el proyecto", EditorStyles.boldLabel);

        // Busca todos los archivos .json recursivamente dentro de Application.dataPath
        string[] allJsons = Directory.GetFiles(Application.dataPath, "*.json", SearchOption.AllDirectories);
        foreach (var file in allJsons)
        {
            // Convierte la ruta absoluta a ruta relativa tipo "Assets/..."
            string relativePath = "Assets" + file.Replace(Application.dataPath, "").Replace("\\", "/");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(relativePath);                                          // Muestra la ruta relativa
            if (GUILayout.Button("Abrir", GUILayout.Width(80)))
                jsonFilePath = relativePath;                                        // Asigna la ruta seleccionada al campo de la ventana
            if (!string.IsNullOrEmpty(jsonFilePath))
                LoadJson(jsonFilePath);                                             // Carga el JSON si hay ruta
            EditorGUILayout.EndHorizontal();
        }
    }

    // Lee el archivo JSON de la ruta (path) y parsea su contenido a JObject.
    void LoadJson(string path)
    {
        try
        {
            jsonText = File.ReadAllText(path);                                      // Lee todo el archivo como texto
            jsonObject = JObject.Parse(jsonText);                                   // Parsea a JObject para vista estructurada
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error al leer JSON: " + e.Message);                     // Log en caso de fallo
            jsonObject = null;                                                      // Resetea el objeto si hubo error
        }
    }

    // Guarda el texto crudo (jsonText) en el archivo jsonFilePath verificando que sea JSON válido.
    void SaveJson()
    {
        try
        {
            JToken.Parse(jsonText);                                                 // Valida que el texto sea JSON correcto (lanzará excepción si no lo es)
            File.WriteAllText(jsonFilePath, jsonText);                              // Escribe el texto en disco
            AssetDatabase.Refresh();                                                // Refresca assets
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error al guardar JSON: " + e.Message); // Muestra error si no pudo parsear o escribir
        }
    }

    // Dibuja recursivamente un JObject en la interfaz de la ventana para visualizarlo de forma jerárquica.
    void DrawJsonObject(JObject obj, int indent)
    {
        foreach (var pair in obj)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(indent * 20); // Sangría según el nivel (indent)

            // Si el valor es otro JObject (objeto anidado), muestra carpeta y llama recursivamente.
            if (pair.Value is JObject childObj)
            {
                GUILayout.Label("📁 " + pair.Key, EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
                DrawJsonObject(childObj, indent + 1);
            }
            // Si el valor es un array, lo delega a DrawJsonArray para mostrar su contenido.
            else if (pair.Value is JArray array)
            {
                GUILayout.Label($"📦 {pair.Key} [Array]", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
                DrawJsonArray(array, indent + 1);
            }
            // Si es un valor primitivo (string, número, bool...), lo muestra en una sola línea.
            else
            {
                GUILayout.Label($"{pair.Key}: {pair.Value}");
                EditorGUILayout.EndHorizontal();
            }
        }
    }

    // Dibuja recursivamente un JArray en la interfaz mostrando cada elemento (objeto o valor).
    private void DrawJsonArray(JArray array, int indent)
    {
        for (int i = 0; i < array.Count; i++)
        {
            var val = array[i];
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(indent * 20); // Sangría para el elemento del array

            // Si el elemento es un objeto, lo presenta y lo dibuja recursivamente.
            if (val is JObject childObj)
            {
                GUILayout.Label($"Elemento {i}", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
                DrawJsonObject(childObj, indent + 1);
            }
            // Si el elemento es un valor simple, lo muestra con su índice.
            else
            {
                GUILayout.Label($"[{i}] {val}");
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
