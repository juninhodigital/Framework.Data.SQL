USE DB_SYSADV
GO
 
IF EXISTS(SELECT NAME FROM sys.objects WHERE name = 'FN_SPLIT_STRING' AND type='IF')
BEGIN
 	DROP FUNCTION FN_SPLIT_STRING
END
GO

CREATE FUNCTION FN_SPLIT_STRING
(
    @List NVARCHAR(MAX),
    @Delim VARCHAR(255)
)
RETURNS TABLE
AS
    RETURN ( SELECT [Value] FROM 
        ( 
        SELECT 
            [Value] = LTRIM(RTRIM(SUBSTRING(@List, [Number],
            CHARINDEX(@Delim, @List + @Delim, [Number]) - [Number])))
        FROM (SELECT Number = ROW_NUMBER() OVER (ORDER BY name)
            FROM sys.all_objects) AS x
            WHERE Number <= LEN(@List)
            AND SUBSTRING(@Delim + @List, [Number], LEN(@Delim)) = @Delim
        ) AS y
);

GRANT SELECT ON FN_SPLIT_STRING TO PUBLIC 
GO