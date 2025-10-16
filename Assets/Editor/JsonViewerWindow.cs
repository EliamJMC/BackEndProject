using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class JsonManagerWindow : EditorWindow
{
    // Modo actual de la ventana (Crear modelo, Crear JSON o Ver JSON)
    private enum JsonMode { CrearModelo, CrearJsonDesdeModelo, VerJson }
    private JsonMode currentMode = JsonMode.CrearModelo;

    // Variables comunes
    private Vector2 scrollPos;
    private string basePath => Path.Combine(Application.dataPath, "JsonModels"); // Carpeta base de modelos

    // --- Crear modelo ---
    private string modelName = "";
    private List<KeyValuePair<string, string>> modelFields = new List<KeyValuePair<string, string>>();

    // --- Crear JSON desde modelo ---
    private string currentJsonModelName = null;
    private string[] availableModels;
    private int selectedModelIndex = 0;
    private JObject modelTemplate;
    private Dictionary<string, string> jsonValues = new Dictionary<string, string>();

    // --- Ver/Editar JSON ---
    private string jsonFilePath = "";
    private string jsonText = "";
    private JObject jsonObject;
    private bool isEditing = false;

    [MenuItem("Tools/JSON Manager")]
    public static void ShowWindow()
    {
        GetWindow<JsonManagerWindow>("JSON Manager");
    }

    private void OnEnable()
    {
        // Crea carpeta base si no existe
        if (!Directory.Exists(basePath))
            Directory.CreateDirectory(basePath);

        // Carga lista de modelos
        LoadAvailableModels();
    }

    private void OnGUI()
    {
        GUILayout.Label("🧩 JSON Manager", EditorStyles.boldLabel);
        GUILayout.Space(5);

        // Barra de modos
        GUILayout.BeginHorizontal();
        if (GUILayout.Toggle(currentMode == JsonMode.CrearModelo, "Crear Modelo", EditorStyles.toolbarButton))
            currentMode = JsonMode.CrearModelo;
        if (GUILayout.Toggle(currentMode == JsonMode.CrearJsonDesdeModelo, "Crear JSON desde Modelo", EditorStyles.toolbarButton))
            currentMode = JsonMode.CrearJsonDesdeModelo;
        if (GUILayout.Toggle(currentMode == JsonMode.VerJson, "Leer / Editar JSON", EditorStyles.toolbarButton))
            currentMode = JsonMode.VerJson;
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // Cambiar contenido según el modo actual
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

    // ============================================================
    // 🧩 MODO 1: Crear Modelo
    // ============================================================
    private void DrawCreateModel()
    {
        GUILayout.Label("Crear o editar modelo JSON", EditorStyles.boldLabel);

        // 🔹 Crear o editar modelo actual
        GUILayout.Space(5);
        GUILayout.Label("Editar / Crear nuevo modelo:", EditorStyles.boldLabel);
        modelName = EditorGUILayout.TextField("Nombre del modelo:", modelName);

        GUILayout.Space(5);
        GUILayout.Label("Campos del modelo (nombre + valor por defecto):", EditorStyles.miniBoldLabel);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
        for (int i = 0; i < modelFields.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            modelFields[i] = new KeyValuePair<string, string>(
                EditorGUILayout.TextField(modelFields[i].Key),
                EditorGUILayout.TextField(modelFields[i].Value)
            );
            if (GUILayout.Button("X", GUILayout.Width(20)))
                modelFields.RemoveAt(i);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        // 🔹 Mostrar lista de modelos existentes
        if (availableModels != null && availableModels.Length > 0)
        {
            GUILayout.Label("Modelos existentes:", EditorStyles.miniBoldLabel);
            for (int i = 0; i < availableModels.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(availableModels[i]);
                if (GUILayout.Button("Editar", GUILayout.Width(80)))
                {
                    LoadModelForEditing(availableModels[i]);
                }
                if (GUILayout.Button("🗑️", GUILayout.Width(30)))
                {
                    if (EditorUtility.DisplayDialog("Eliminar modelo",
                        $"¿Seguro que deseas eliminar '{availableModels[i]}'?", "Sí", "No"))
                    {
                        string modelPath = Path.Combine(basePath, availableModels[i] + ".jsonmodel");
                        File.Delete(modelPath);
                        AssetDatabase.Refresh();
                        LoadAvailableModels();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.Space(10);
        }

        if (GUILayout.Button("Agregar Campo"))
            modelFields.Add(new KeyValuePair<string, string>("", ""));

        GUILayout.Space(10);

        if (GUILayout.Button("💾 Guardar Modelo"))
            SaveModel();
    }

    private void LoadModelForEditing(string modelToLoad)
    {
        try
        {
            string path = Path.Combine(basePath, modelToLoad + ".jsonmodel");
            string content = File.ReadAllText(path);
            JObject obj = JObject.Parse(content);

            modelName = modelToLoad;
            modelFields.Clear();

            foreach (var prop in obj)
                modelFields.Add(new KeyValuePair<string, string>(prop.Key, prop.Value.ToString()));
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al cargar modelo: {e.Message}");
        }
    }

    private void SaveModel()
    {
        if (string.IsNullOrEmpty(modelName))
        {
            EditorUtility.DisplayDialog("Error", "Debes ingresar un nombre para el modelo.", "OK");
            return;
        }

        JObject obj = new JObject();
        foreach (var field in modelFields)
            obj[field.Key] = field.Value;

        string path = Path.Combine(basePath, modelName + ".jsonmodel");
        File.WriteAllText(path, obj.ToString());
        AssetDatabase.Refresh();

        LoadAvailableModels();

        EditorUtility.DisplayDialog("Éxito", $"Modelo '{modelName}' guardado correctamente.", "OK");
    }

    // ============================================================
    // 🏗️ MODO 2: Crear JSON desde Modelo
    // ============================================================
    private void DrawCreateFromModel()
    {
        GUILayout.Label("Crear JSON a partir de un modelo", EditorStyles.boldLabel);

        if (availableModels == null || availableModels.Length == 0)
        {
            EditorGUILayout.HelpBox("No hay modelos guardados. Crea uno primero.", MessageType.Info);
            return;
        }

        selectedModelIndex = EditorGUILayout.Popup("Modelo:", selectedModelIndex, availableModels);

        if (GUILayout.Button("Cargar Modelo"))
            LoadModelTemplate(availableModels[selectedModelIndex]);

        if (modelTemplate != null)
        {
            GUILayout.Space(10);
            GUILayout.Label("Completa los valores:", EditorStyles.miniBoldLabel);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(300));
            foreach (var prop in modelTemplate)
            {
                if (!jsonValues.ContainsKey(prop.Key))
                    jsonValues[prop.Key] = prop.Value.ToString();

                jsonValues[prop.Key] = EditorGUILayout.TextField(prop.Key, jsonValues[prop.Key]);
            }
            EditorGUILayout.EndScrollView();

            GUILayout.Space(10);
            if (GUILayout.Button("💾 Guardar JSON"))
                SaveJsonFromModel();

            if (!string.IsNullOrEmpty(jsonFilePath))
            {
                GUILayout.Space(10);
                if (GUILayout.Button("💾 Guardar Cambios en JSON Existente"))
                    SaveExistingJson();
            }

            GUILayout.Space(10);
            GUILayout.Label("Editar JSON existente basado en modelo", EditorStyles.boldLabel);

            if (GUILayout.Button("📂 Cargar JSON existente"))
            {
                string path = EditorUtility.OpenFilePanel("Selecciona un JSON existente", Application.dataPath, "json");
                if (!string.IsNullOrEmpty(path))
                    LoadJsonForModelEditing(path);
            }

        }
    }

    private void SaveExistingJson()
    {
        if (string.IsNullOrEmpty(jsonFilePath) || string.IsNullOrEmpty(currentJsonModelName))
            return;

        JObject updated = new JObject();

        foreach (var pair in jsonValues)
            updated[pair.Key] = pair.Value;

        updated["__modelName"] = currentJsonModelName;

        File.WriteAllText(jsonFilePath, updated.ToString());
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Éxito", "Cambios guardados correctamente en el JSON.", "OK");
    }


    private void LoadJsonForModelEditing(string path)
    {
        try
        {
            string content = File.ReadAllText(path);
            JObject loaded = JObject.Parse(content);

            // 🔹 Verificar que tenga el campo "__modelName"
            if (!loaded.ContainsKey("__modelName"))
            {
                EditorUtility.DisplayDialog("Error", "Este JSON no tiene información del modelo base.", "OK");
                return;
            }

            currentJsonModelName = loaded["__modelName"].ToString();
            LoadModelTemplate(currentJsonModelName);

            // 🔹 Llenar valores actuales
            jsonValues.Clear();
            foreach (var prop in modelTemplate)
            {
                if (loaded.ContainsKey(prop.Key))
                    jsonValues[prop.Key] = loaded[prop.Key].ToString();
                else
                    jsonValues[prop.Key] = prop.Value.ToString();
            }

            // Guardar ruta del JSON que se está editando
            jsonFilePath = path;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error al cargar JSON para edición: " + e.Message);
        }
    }


    private void LoadAvailableModels()
    {
        var files = Directory.GetFiles(basePath, "*.jsonmodel");
        availableModels = new string[files.Length];
        for (int i = 0; i < files.Length; i++)
            availableModels[i] = Path.GetFileNameWithoutExtension(files[i]);
    }

    private void LoadModelTemplate(string modelName)
    {
        string path = Path.Combine(basePath, modelName + ".jsonmodel");
        string content = File.ReadAllText(path);
        modelTemplate = JObject.Parse(content);
        jsonValues.Clear();
    }

    private void SaveJsonFromModel()
    {
        JObject newJson = new JObject();

        // 🔹 Guardamos los valores
        foreach (var pair in jsonValues)
            newJson[pair.Key] = pair.Value;

        // 🔹 Agregamos un campo oculto para identificar el modelo
        newJson["__modelName"] = availableModels[selectedModelIndex];

        string savePath = EditorUtility.SaveFilePanel("Guardar JSON", Application.dataPath, "NuevoArchivo", "json");
        if (!string.IsNullOrEmpty(savePath))
        {
            File.WriteAllText(savePath, newJson.ToString());
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Éxito", "Archivo JSON creado correctamente.", "OK");
        }
    }


    // ============================================================
    // 👀 MODO 3: Ver / Editar JSON
    // ============================================================
    private void DrawJsonViewer()
    {
        GUILayout.Label("Ver / Editar JSON", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Seleccionar JSON", GUILayout.Width(150)))
        {
            jsonFilePath = EditorUtility.OpenFilePanel("Selecciona un archivo JSON", Application.dataPath, "json");
            if (!string.IsNullOrEmpty(jsonFilePath))
                LoadJson(jsonFilePath);
        }

        if (!string.IsNullOrEmpty(jsonFilePath))
            GUILayout.Label(Path.GetFileName(jsonFilePath));
        GUILayout.EndHorizontal();

        if (jsonObject != null)
        {
            if (GUILayout.Button(isEditing ? "💾 Guardar y Volver" : "✏️ Editar JSON", GUILayout.Height(25)))
            {
                if (isEditing)
                {
                    SaveJson();
                    LoadJson(jsonFilePath);
                    isEditing = false;
                }
                else
                    isEditing = true;
            }

            GUILayout.Space(10);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            if (isEditing)
                jsonText = EditorGUILayout.TextArea(jsonText, GUILayout.ExpandHeight(true));
            else
                DrawJsonObject(jsonObject, 0);
            EditorGUILayout.EndScrollView();
        }

        DrawJsonListSection();
    }

    void DrawJsonListSection()
    {
        GUILayout.Label("📜 Lista completa de JSON en el proyecto", EditorStyles.boldLabel);

        string[] allJsons = Directory.GetFiles(Application.dataPath, "*.json", SearchOption.AllDirectories);
        foreach (var file in allJsons)
        {
            string relativePath = "Assets" + file.Replace(Application.dataPath, "").Replace("\\", "/");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(relativePath);
            if (GUILayout.Button("Abrir", GUILayout.Width(80)))
                jsonFilePath = relativePath;
            if (!string.IsNullOrEmpty(jsonFilePath))
                LoadJson(jsonFilePath);
            EditorGUILayout.EndHorizontal();
        }
    }

    void LoadJson(string path)
    {
        try
        {
            jsonText = File.ReadAllText(path);
            jsonObject = JObject.Parse(jsonText);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error al leer JSON: " + e.Message);
            jsonObject = null;
        }
    }

    void SaveJson()
    {
        try
        {
            JToken.Parse(jsonText);
            File.WriteAllText(jsonFilePath, jsonText);
            AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error al guardar JSON: " + e.Message);
        }
    }

    void DrawJsonObject(JObject obj, int indent)
    {
        foreach (var pair in obj)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(indent * 20);

            if (pair.Value is JObject childObj)
            {
                GUILayout.Label("📁 " + pair.Key, EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
                DrawJsonObject(childObj, indent + 1);
            }
            else if (pair.Value is JArray array)
            {
                GUILayout.Label($"📦 {pair.Key} [Array]", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
                DrawJsonArray(array, indent + 1);
            }
            else
            {
                GUILayout.Label($"{pair.Key}: {pair.Value}");
                EditorGUILayout.EndHorizontal();
            }
        }
    }

    private void DrawJsonArray(JArray array, int indent)
    {
        for (int i = 0; i < array.Count; i++)
        {
            var val = array[i];
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(indent * 20);

            if (val is JObject childObj)
            {
                GUILayout.Label($"Elemento {i}", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
                DrawJsonObject(childObj, indent + 1);
            }
            else
            {
                GUILayout.Label($"[{i}] {val}");
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
