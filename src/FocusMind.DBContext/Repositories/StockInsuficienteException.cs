namespace FocusMind.DBContext.Repositories;

// Traduce el THROW 51001 de usp_InsertarPedidoDetalle (stock insuficiente) a una excepción de
// dominio, para que FocusMind.Business no necesite conocer Microsoft.Data.SqlClient ni números
// de error T-SQL específicos — mismo criterio ya usado en ProductoRepository.ActualizarAsync
// (HU-15), que traduce el THROW 51002 a un simple `false`.
public sealed class StockInsuficienteException(string mensaje) : Exception(mensaje);
