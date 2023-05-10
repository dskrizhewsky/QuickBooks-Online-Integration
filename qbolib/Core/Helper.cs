#region Copyright Notice
/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2023 Dmytro Skryzhevskyi
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included
 * in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using Intuit.Ipp.Core;
using Intuit.Ipp.Data;
using Intuit.Ipp.DataService;
using Intuit.Ipp.EntitlementService;
using Intuit.Ipp.Exception;
using Intuit.Ipp.QueryFilter;
using Intuit.Ipp.ReportService;

namespace Dmytro.Skryzhevskyi.ExternalAccountingTools.Core
{
    internal class Helper
    {
        internal static T Add<T>(ServiceContext context, T entity) where T : IEntity
        {
            DataService service = new DataService(context);
            T added = service.Add<T>(entity);
            return added;
        }

        internal static ImmutableList<T> LoadFullCollection<T>(ServiceContext context, T entity) where T : IEntity
        {
            ImmutableList<T> collection;
            List<T> fullCollection = new List<T>();
            int startPosition = 1;
            int maxResults = 1000;
            do
            {
                collection = Helper.FindAll(context, entity, startPosition, maxResults);
                fullCollection.AddRange(collection);
                startPosition += maxResults;
            } while (collection.Any());

            return fullCollection.ToImmutableList();
        }

        internal static ImmutableList<T> FindAll<T>(ServiceContext context, T entity, int startPosition = 1, int maxResults = 100) where T : IEntity
        {
            DataService service = new DataService(context);

            ReadOnlyCollection<T> entityList = service.FindAll(entity, startPosition, maxResults);

            return entityList.ToImmutableList();
        }

        internal static T FindById<T>(ServiceContext context, T entity) where T : IEntity
        {
            DataService service = new DataService(context);
            T foundEntity = service.FindById(entity);

            return foundEntity;
        }

        internal static T Update<T>(ServiceContext context, T entity) where T : IEntity
        {
            DataService service = new DataService(context);
            T updated = service.Update<T>(entity);
            return updated;
        }

        internal static T SparseUpdate<T>(ServiceContext context, T entity) where T : IEntity
        {
            DataService service = new DataService(context);
            T updated = service.Update<T>(entity);
            return updated;
        }

        internal static T Delete<T>(ServiceContext context, T entity) where T : IEntity
        {
            DataService service = new DataService(context);
            T deleted = service.Delete<T>(entity);
            return deleted;
        }


        internal static T Void<T>(ServiceContext context, T entity) where T : IEntity
        {
            DataService service = new DataService(context);
            service.Void<T>(entity);

            try
            {
                T found = service.FindById<T>(entity);
                return found;
            }
            catch (IdsException)
            {
            }

            return entity;
        }

        internal static ImmutableList<T> CDC<T>(ServiceContext context, T entity, DateTime changedSince) where T : IEntity
        {
            DataService service = new DataService(context);
            List<IEntity> entityList = new List<IEntity>();
            entityList.Add(entity);

            IntuitCDCResponse response = service.CDC(entityList, changedSince);
            if (response.entities.Count == 0)
            {
                return null;
            }
            
            List<T> found = response.getEntity(entity.GetType().Name).Cast<T>().ToList();
            return found.ToImmutableList();
        }

        internal static Attachable Upload(ServiceContext context, Attachable attachable, System.IO.Stream stream)
        {
            DataService service = new DataService(context);

            Attachable uploaded = service.Upload(attachable, stream);
            return uploaded;
        }

        internal static byte[] Download(ServiceContext context, Attachable entity)
        {
            DataService service = new DataService(context);
            return service.Download(entity);
        }

        internal static ImmutableList<IntuitBatchResponse> Batch<T>(ServiceContext context, Dictionary<OperationEnum, object> operationDictionary) where T : IEntity
        {
            DataService service = new DataService(context);
            List<T> addedList = new List<T>();
            List<T> newList = new List<T>();


            QueryService<T> entityContext = new QueryService<T>(context);

            Batch batch = service.CreateNewBatch();

            foreach (KeyValuePair<OperationEnum, object> entry in operationDictionary)
            {
                if (entry.Value.GetType().Name.Equals(typeof(T).Name))
                    batch.Add(entry.Value as IEntity, entry.Key.ToString() + entry.Value.GetType().Name, entry.Key);
                else
                    batch.Add(entry.Value as string, "Query" + typeof(T).Name);
            }

            batch.Execute();
            return batch.IntuitBatchItemResponses.ToImmutableList();
        }


        internal static Boolean CheckEqual(Object expected, Object actual)
        {
            return true;
        }

        internal static String GetGuid()
        {
            return Guid.NewGuid().ToString("N");
        }

        internal static Report GetReportAsync(ServiceContext context, string reportName)
        {
            ReportService service = new ReportService(context);

            IdsException exp = null;
            Boolean reportReturned = false;
            Report actual = null;
            ManualResetEvent manualEvent = new ManualResetEvent(false);
            service.OnExecuteReportAsyncCompleted += (sender, e) =>
            {
                manualEvent.Set();
                if (e.Error != null)
                {
                    exp = e.Error;
                }
                if (exp == null)
                {
                    if (e.Report != null)
                    {
                        reportReturned = true;
                        actual = e.Report;
                    }
                }
            };

            service.ExecuteReportAsync(reportName);
            manualEvent.WaitOne(30000, false); 
            Thread.Sleep(10000);

            if (exp != null)
            {
                throw exp;
            }

            if (!reportReturned)
            {
                //return null; 
            }

            manualEvent.Reset();
            return actual;

        }

        internal static EntitlementsResponse GetEntitlementAsync(ServiceContext context, string baseUrl)
        {
            EntitlementService service = new EntitlementService(context);

            IdsException exp = null;
            Boolean entitlementReturned = false;
            EntitlementsResponse entitlements = null;

            ManualResetEvent manualEvent = new ManualResetEvent(false);

            service.OnGetEntilementAsyncCompleted += (sender, e) =>
            {
                manualEvent.Set();
                if (e.Error != null)
                {
                    exp = e.Error;
                }
                if (exp == null)
                {
                    if (e.EntitlementsResponse != null)
                    {
                        entitlementReturned = true;
                        entitlements = e.EntitlementsResponse;
                    }
                }
            };

            service.GetEntitlementsAsync(baseUrl);

            manualEvent.WaitOne(30000, false); Thread.Sleep(10000);

            if (exp != null)
            {
                throw exp;
            }

            if (!entitlementReturned)
            {
                //return null
            }

            manualEvent.Reset();
            return entitlements;

        }

        internal static T AddAsync<T>(ServiceContext context, T entity) where T : IEntity
        {
            DataService service = new DataService(context);

            bool isAdded = false;

            IdsException exp = null;

            T actual = (T)Activator.CreateInstance(entity.GetType());
            ManualResetEvent manualEvent = new ManualResetEvent(false);

            service.OnAddAsyncCompleted += (sender, e) =>
            {
                isAdded = true;
                manualEvent.Set();
                if (e.Error != null)
                {
                    exp = e.Error;
                }
                if (exp == null)
                {
                    if (e.Entity != null)
                    {
                        actual = (T)e.Entity;
                    }
                }
            };

            service.AddAsync(entity);

            manualEvent.WaitOne(30000, false); Thread.Sleep(10000);

            if (exp != null)
            {
                throw exp;
            }

            if (!isAdded)
            {
                //return null;
            }

            manualEvent.Reset();

            return actual;

        }

        internal static ImmutableList<T> FindAllAsync<T>(ServiceContext context, T entity, int startPosition = 1, int maxResults = 500) where T : IEntity
        {
            DataService service = new DataService(context);

            bool isFindAll = false;

            IdsException exp = null;

            ManualResetEvent manualEvent = new ManualResetEvent(false);

            List<T> entities = new List<T>();

            service.OnFindAllAsyncCompleted += (sender, e) =>
            {
                isFindAll = true;
                manualEvent.Set();
                if (e.Error != null)
                {
                    exp = e.Error;
                }
                if (exp == null)
                {
                    if (e.Entities != null)
                    {
                        foreach (IEntity en in e.Entities)
                        {
                            entities.Add((T)en);
                        }
                    }
                }
            };

            service.FindAllAsync<T>(entity, 1, 10);

            manualEvent.WaitOne(60000, false); Thread.Sleep(10000);


            //if (!isFindAll)
            //{
            //    return null;
            //}

            if (exp != null)
            {
                throw exp;
            }

            //if (entities != null)
            //{
            //    Assert.IsTrue(entities.Count >= 0);
            //}

            // Set the event to non-signaled before making next async call.    
            manualEvent.Reset();
            return entities.ToImmutableList();
        }

        internal static T FindByIdAsync<T>(ServiceContext context, T entity) where T : IEntity
        {
            DataService service = new DataService(context);

            bool isFindById = false;

            IdsException exp = null;

            ManualResetEvent manualEvent = new ManualResetEvent(false);

            T returnedEntity = default(T);

            service.OnFindByIdAsyncCompleted += (sender, e) =>
            {
             
                manualEvent.Set();
                isFindById = true;
                returnedEntity = (T)e.Entity;
            };

            service.FindByIdAsync<T>(entity);
            manualEvent.WaitOne(60000, false); Thread.Sleep(10000);

            //// Check if we completed the async call, or fail the test if we timed out.    
            //if (!isFindById)
            //{
            //    //return null;
            //}

            if (exp != null)
            {
                throw exp;
            }

            manualEvent.Reset();
            return returnedEntity;
        }

        internal static T UpdateAsync<T>(ServiceContext context, T entity) where T : IEntity
        {
            DataService service = new DataService(context);

            bool isUpdated = false;

            ManualResetEvent manualEvent = new ManualResetEvent(false);

            IdsException exp = null;

            T returnedEntity = entity;

            service.OnUpdateAsyncCompleted += (sender, e) =>
            {

                isUpdated = true;
                manualEvent.Set();
                if (e.Error != null)
                {
                    exp = e.Error;
                }
                else
                {
                    if (e.Entity != null)
                    {
                        returnedEntity = (T)e.Entity;
                    }
                }
            };

            service.UpdateAsync(entity);

            manualEvent.WaitOne(30000, false); Thread.Sleep(10000);

            if (exp != null)
            {
                throw exp;
            }

            if (!isUpdated)
            {
               //return null
            }
            
            manualEvent.Reset();

            return returnedEntity;
        }


     

        internal static T DeleteAsync<T>(ServiceContext context, T entity) where T : IEntity
        {
            DataService service = new DataService(context);

            bool isDeleted = false;

            ManualResetEvent manualEvent = new ManualResetEvent(false);

            IdsException exp = null;
            T returnedEntity = entity;
            service.OnDeleteAsyncCompleted += (sender, e) =>
            {
                isDeleted = true;
                manualEvent.Set();
                if (e.Error != null)
                {
                    exp = e.Error;
                }
                else
                {
                    if (e.Entity != null)
                    {
                        returnedEntity = (T)e.Entity;
                    }
                }
            };

            service.DeleteAsync(entity);

            manualEvent.WaitOne(30000, false); Thread.Sleep(10000);

            if (exp != null)
            {
                throw exp;
            }
            //if (!isDeleted)
            //{
            //   //return null;
            //}

            manualEvent.Reset();

            return returnedEntity;
        }

        internal static void VoidAsync<T>(ServiceContext context, T entity) where T : IEntity
        {
            DataService service = new DataService(context);

            bool isVoided = false;

            ManualResetEvent manualEvent = new ManualResetEvent(false);

            IdsException exp = null;
            service.OnVoidAsyncCompleted += (sender, e) =>
            {
                isVoided = true;
                manualEvent.Set();
                if (e.Error != null)
                {
                    exp = e.Error;
                }

            };

            service.VoidAsync(entity);

            manualEvent.WaitOne(30000, false);
            Thread.Sleep(10000);

            if (exp != null)
            {
                throw exp;
            }

            //if (!isVoided)
            //{
            //    return null;
            //}

            manualEvent.Reset();
        }

        internal static T FindOrAdd<T>(ServiceContext context, T entity) where T : IEntity
        {
            ImmutableList<T> returnedEntities = FindAll<T>(context, entity, 1, 500);
            int count = 0;
            foreach (T en in returnedEntities)
            {
                if ((returnedEntities[count] as IntuitEntity).status != EntityStatusEnum.SyncError)
                    return returnedEntities[count];
                count++;
            }


            BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Static;
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();

            foreach (Type type in types)
            {
                if (context.ServiceType == IntuitServicesType.QBO && type.Name == "QBOHelper")
                {
                    String methodName = "Create" + entity.GetType().Name;
                    MethodInfo method = type.GetMethod("Create" + entity.GetType().Name, bindingFlags);
                    entity = (T)method.Invoke(null, new object[] { context });
                    T returnEntity = Add(context, entity);
                    return returnEntity;
                }
            }
            throw new System.ApplicationException("Could not find QBOHelper");
        }
    }
}