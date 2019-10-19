﻿//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2020 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using GameFramework.Config;
using GameFramework.Resource;
using System.IO;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 全局配置组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Config")]
    public sealed class ConfigComponent : GameFrameworkComponent
    {
        private const int DefaultPriority = 0;

        private IConfigManager m_ConfigManager = null;
        private EventComponent m_EventComponent = null;

        [SerializeField]
        private bool m_EnableLoadConfigUpdateEvent = false;

        [SerializeField]
        private bool m_EnableLoadConfigDependencyAssetEvent = false;

        [SerializeField]
        private string m_ConfigHelperTypeName = "UnityGameFramework.Runtime.DefaultConfigHelper";

        [SerializeField]
        private ConfigHelperBase m_CustomConfigHelper = null;

        /// <summary>
        /// 获取全局配置数量。
        /// </summary>
        public int ConfigCount
        {
            get
            {
                return m_ConfigManager.ConfigCount;
            }
        }

        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_ConfigManager = GameFrameworkEntry.GetModule<IConfigManager>();
            if (m_ConfigManager == null)
            {
                Log.Fatal("Config manager is invalid.");
                return;
            }

            m_ConfigManager.LoadConfigSuccess += OnLoadConfigSuccess;
            m_ConfigManager.LoadConfigFailure += OnLoadConfigFailure;

            if (m_EnableLoadConfigUpdateEvent)
            {
                m_ConfigManager.LoadConfigUpdate += OnLoadConfigUpdate;
            }

            if (m_EnableLoadConfigDependencyAssetEvent)
            {
                m_ConfigManager.LoadConfigDependencyAsset += OnLoadConfigDependencyAsset;
            }
        }

        private void Start()
        {
            BaseComponent baseComponent = GameEntry.GetComponent<BaseComponent>();
            if (baseComponent == null)
            {
                Log.Fatal("Base component is invalid.");
                return;
            }

            m_EventComponent = GameEntry.GetComponent<EventComponent>();
            if (m_EventComponent == null)
            {
                Log.Fatal("Event component is invalid.");
                return;
            }

            if (baseComponent.EditorResourceMode)
            {
                m_ConfigManager.SetResourceManager(baseComponent.EditorResourceHelper);
            }
            else
            {
                m_ConfigManager.SetResourceManager(GameFrameworkEntry.GetModule<IResourceManager>());
            }

            ConfigHelperBase configHelper = Helper.CreateHelper(m_ConfigHelperTypeName, m_CustomConfigHelper);
            if (configHelper == null)
            {
                Log.Error("Can not create config helper.");
                return;
            }

            configHelper.name = "Config Helper";
            Transform transform = configHelper.transform;
            transform.SetParent(this.transform);
            transform.localScale = Vector3.one;

            m_ConfigManager.SetConfigHelper(configHelper);
        }

        /// <summary>
        /// 加载全局配置。
        /// </summary>
        /// <param name="configName">全局配置名称。</param>
        /// <param name="configAssetName">全局配置资源名称。</param>
        /// <param name="loadType">全局配置加载方式。</param>
        public void LoadConfig(string configName, string configAssetName, LoadType loadType)
        {
            LoadConfig(configName, configAssetName, loadType, DefaultPriority, null);
        }

        /// <summary>
        /// 加载全局配置。
        /// </summary>
        /// <param name="configName">全局配置名称。</param>
        /// <param name="configAssetName">全局配置资源名称。</param>
        /// <param name="loadType">全局配置加载方式。</param>
        /// <param name="priority">加载全局配置资源的优先级。</param>
        public void LoadConfig(string configName, string configAssetName, LoadType loadType, int priority)
        {
            LoadConfig(configName, configAssetName, loadType, priority, null);
        }

        /// <summary>
        /// 加载全局配置。
        /// </summary>
        /// <param name="configName">全局配置名称。</param>
        /// <param name="configAssetName">全局配置资源名称。</param>
        /// <param name="loadType">全局配置加载方式。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void LoadConfig(string configName, string configAssetName, LoadType loadType, object userData)
        {
            LoadConfig(configName, configAssetName, loadType, DefaultPriority, userData);
        }

        /// <summary>
        /// 加载全局配置。
        /// </summary>
        /// <param name="configName">全局配置名称。</param>
        /// <param name="configAssetName">全局配置资源名称。</param>
        /// <param name="loadType">全局配置加载方式。</param>
        /// <param name="priority">加载全局配置资源的优先级。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void LoadConfig(string configName, string configAssetName, LoadType loadType, int priority, object userData)
        {
            if (string.IsNullOrEmpty(configName))
            {
                Log.Error("Config name is invalid.");
                return;
            }

            m_ConfigManager.LoadConfig(configAssetName, loadType, priority, LoadConfigInfo.Create(configName, userData));
        }

        /// <summary>
        /// 解析全局配置。
        /// </summary>
        /// <param name="text">要解析的全局配置文本。</param>
        /// <returns>是否解析全局配置成功。</returns>
        public bool ParseConfig(string text)
        {
            return m_ConfigManager.ParseConfig(text);
        }

        /// <summary>
        /// 解析全局配置。
        /// </summary>
        /// <param name="text">要解析的全局配置文本。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>是否解析全局配置成功。</returns>
        public bool ParseConfig(string text, object userData)
        {
            return m_ConfigManager.ParseConfig(text, userData);
        }

        /// <summary>
        /// 解析全局配置。
        /// </summary>
        /// <param name="bytes">要解析的全局配置二进制流。</param>
        /// <returns>是否解析全局配置成功。</returns>
        public bool ParseConfig(byte[] bytes)
        {
            return m_ConfigManager.ParseConfig(bytes);
        }

        /// <summary>
        /// 解析全局配置。
        /// </summary>
        /// <param name="bytes">要解析的全局配置二进制流。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>是否解析全局配置成功。</returns>
        public bool ParseConfig(byte[] bytes, object userData)
        {
            return m_ConfigManager.ParseConfig(bytes, userData);
        }

        /// <summary>
        /// 解析全局配置。
        /// </summary>
        /// <param name="stream">要解析的全局配置二进制流。</param>
        /// <returns>是否解析全局配置成功。</returns>
        public bool ParseConfig(Stream stream)
        {
            return m_ConfigManager.ParseConfig(stream);
        }

        /// <summary>
        /// 解析全局配置。
        /// </summary>
        /// <param name="stream">要解析的全局配置二进制流。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>是否解析全局配置成功。</returns>
        public bool ParseConfig(Stream stream, object userData)
        {
            return m_ConfigManager.ParseConfig(stream, userData);
        }

        /// <summary>
        /// 检查是否存在指定全局配置项。
        /// </summary>
        /// <param name="configName">要检查全局配置项的名称。</param>
        /// <returns>指定的全局配置项是否存在。</returns>
        public bool HasConfig(string configName)
        {
            return m_ConfigManager.HasConfig(configName);
        }

        /// <summary>
        /// 移除指定全局配置项。
        /// </summary>
        /// <param name="configName">要移除全局配置项的名称。</param>
        public void RemoveConfig(string configName)
        {
            m_ConfigManager.RemoveConfig(configName);
        }

        /// <summary>
        /// 清空所有全局配置项。
        /// </summary>
        public void RemoveAllConfigs()
        {
            m_ConfigManager.RemoveAllConfigs();
        }

        /// <summary>
        /// 从指定全局配置项中读取布尔值。
        /// </summary>
        /// <param name="configName">要获取全局配置项的名称。</param>
        /// <returns>读取的布尔值。</returns>
        public bool GetBool(string configName)
        {
            return m_ConfigManager.GetBool(configName);
        }

        /// <summary>
        /// 从指定全局配置项中读取布尔值。
        /// </summary>
        /// <param name="configName">要获取全局配置项的名称。</param>
        /// <param name="defaultValue">当指定的全局配置项不存在时，返回此默认值。</param>
        /// <returns>读取的布尔值。</returns>
        public bool GetBool(string configName, bool defaultValue)
        {
            return m_ConfigManager.GetBool(configName, defaultValue);
        }

        /// <summary>
        /// 从指定全局配置项中读取整数值。
        /// </summary>
        /// <param name="configName">要获取全局配置项的名称。</param>
        /// <returns>读取的整数值。</returns>
        public int GetInt(string configName)
        {
            return m_ConfigManager.GetInt(configName);
        }

        /// <summary>
        /// 从指定全局配置项中读取整数值。
        /// </summary>
        /// <param name="configName">要获取全局配置项的名称。</param>
        /// <param name="defaultValue">当指定的全局配置项不存在时，返回此默认值。</param>
        /// <returns>读取的整数值。</returns>
        public int GetInt(string configName, int defaultValue)
        {
            return m_ConfigManager.GetInt(configName, defaultValue);
        }

        /// <summary>
        /// 从指定全局配置项中读取浮点数值。
        /// </summary>
        /// <param name="configName">要获取全局配置项的名称。</param>
        /// <returns>读取的浮点数值。</returns>
        public float GetFloat(string configName)
        {
            return m_ConfigManager.GetFloat(configName);
        }

        /// <summary>
        /// 从指定全局配置项中读取浮点数值。
        /// </summary>
        /// <param name="configName">要获取全局配置项的名称。</param>
        /// <param name="defaultValue">当指定的全局配置项不存在时，返回此默认值。</param>
        /// <returns>读取的浮点数值。</returns>
        public float GetFloat(string configName, float defaultValue)
        {
            return m_ConfigManager.GetFloat(configName, defaultValue);
        }

        /// <summary>
        /// 从指定全局配置项中读取字符串值。
        /// </summary>
        /// <param name="configName">要获取全局配置项的名称。</param>
        /// <returns>读取的字符串值。</returns>
        public string GetString(string configName)
        {
            return m_ConfigManager.GetString(configName);
        }

        /// <summary>
        /// 从指定全局配置项中读取字符串值。
        /// </summary>
        /// <param name="configName">要获取全局配置项的名称。</param>
        /// <param name="defaultValue">当指定的全局配置项不存在时，返回此默认值。</param>
        /// <returns>读取的字符串值。</returns>
        public string GetString(string configName, string defaultValue)
        {
            return m_ConfigManager.GetString(configName, defaultValue);
        }

        private void OnLoadConfigSuccess(object sender, GameFramework.Config.LoadConfigSuccessEventArgs e)
        {
            m_EventComponent.Fire(this, LoadConfigSuccessEventArgs.Create(e));
        }

        private void OnLoadConfigFailure(object sender, GameFramework.Config.LoadConfigFailureEventArgs e)
        {
            Log.Warning("Load config failure, asset name '{0}', error message '{1}'.", e.ConfigAssetName, e.ErrorMessage);
            m_EventComponent.Fire(this, LoadConfigFailureEventArgs.Create(e));
        }

        private void OnLoadConfigUpdate(object sender, GameFramework.Config.LoadConfigUpdateEventArgs e)
        {
            m_EventComponent.Fire(this, LoadConfigUpdateEventArgs.Create(e));
        }

        private void OnLoadConfigDependencyAsset(object sender, GameFramework.Config.LoadConfigDependencyAssetEventArgs e)
        {
            m_EventComponent.Fire(this, LoadConfigDependencyAssetEventArgs.Create(e));
        }
    }
}
